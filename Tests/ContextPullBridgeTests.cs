using System.Reflection;
using RimMind.Bridge.RimChat.Bridge;
using RimMind.Bridge.RimChat.Detection;
using RimMind.Bridge.RimChat.Settings;
using Xunit;

namespace RimMind.Bridge.RimChat.Tests
{
    /// <summary>
    /// ContextPullBridge 注册/注销/刷新逻辑和 Truncate 辅助方法单元测试
    /// </summary>
    public class ContextPullBridgeTests
    {
        public ContextPullBridgeTests()
        {
            RimChatDetector.IsRimChatActive = false;
            RimChatDetector.IsRimChatApiAvailable = false;
            BridgeRimChatSettings.Reset();
            RimMind.Presentation.RimMindAPI.ResetCounts();
            // 清除 StubContextKeyRegistry 中的残留数据
            RimMind.Presentation.RimMindAPI.Context.ContextKeys.Clear();
        }

        // ── Truncate 辅助方法测试（通过反射访问 private static 方法）──

        [Fact]
        public void Truncate_短字符串_不截断()
        {
            var method = GetTruncateMethod();
            var result = (string?)method.Invoke(null, new object[] { "hello", 120 });
            Assert.Equal("hello", result);
        }

        [Fact]
        public void Truncate_长字符串_截断并加省略号()
        {
            var method = GetTruncateMethod();
            var longStr = new string('a', 150);
            var result = (string?)method.Invoke(null, new object[] { longStr, 120 });
            Assert.Equal(123, result!.Length); // 120 + "..."
            Assert.EndsWith("...", result);
        }

        [Fact]
        public void Truncate_空字符串_返回空字符串()
        {
            var method = GetTruncateMethod();
            var result = (string?)method.Invoke(null, new object[] { "", 120 });
            Assert.Equal("", result);
        }

        [Fact]
        public void Truncate_null字符串_返回null()
        {
            var method = GetTruncateMethod();
            var result = (string?)method.Invoke(null, new object?[] { null, 120 });
            Assert.Null(result);
        }

        [Fact]
        public void Truncate_恰好等于maxLen_不截断()
        {
            var method = GetTruncateMethod();
            var str = new string('x', 120);
            var result = (string?)method.Invoke(null, new object[] { str, 120 });
            Assert.Equal(str, result);
        }

        // ── Register 逻辑测试 ──

        [Fact]
        public void Register_RimChat不活跃_不注册任何Provider()
        {
            RimChatDetector.IsRimChatActive = false;
            ContextPullBridge.Register();
            Assert.Equal(0, RimMind.Presentation.RimMindAPI.ContextRegisterCount);
        }

        [Fact]
        public void Register_RimChat活跃且enableContextPull开启_注册Provider()
        {
            RimChatDetector.IsRimChatActive = true;
            var settings = BridgeRimChatSettings.Get();
            settings.enableContextPull = true;
            settings.pullDiplomacyHistory = true;
            settings.pullRpgHistory = true;

            ContextPullBridge.Register();
            // 应注册 rimchat_diplomacy 和 rimchat_rpg_history 两个provider
            Assert.Equal(2, RimMind.Presentation.RimMindAPI.ContextRegisterCount);
        }

        [Fact]
        public void Register_enableContextPull关闭_不注册Provider()
        {
            RimChatDetector.IsRimChatActive = true;
            var settings = BridgeRimChatSettings.Get();
            settings.enableContextPull = false;

            ContextPullBridge.Register();
            Assert.Equal(0, RimMind.Presentation.RimMindAPI.ContextRegisterCount);
        }

        [Fact]
        public void Register_仅开启pullDiplomacyHistory_只注册1个Provider()
        {
            RimChatDetector.IsRimChatActive = true;
            var settings = BridgeRimChatSettings.Get();
            settings.enableContextPull = true;
            settings.pullDiplomacyHistory = true;
            settings.pullRpgHistory = false;

            ContextPullBridge.Register();
            Assert.Equal(1, RimMind.Presentation.RimMindAPI.ContextRegisterCount);
        }

        [Fact]
        public void Register_仅开启pullRpgHistory_只注册1个Provider()
        {
            RimChatDetector.IsRimChatActive = true;
            var settings = BridgeRimChatSettings.Get();
            settings.enableContextPull = true;
            settings.pullDiplomacyHistory = false;
            settings.pullRpgHistory = true;

            ContextPullBridge.Register();
            Assert.Equal(1, RimMind.Presentation.RimMindAPI.ContextRegisterCount);
        }

        // ── Unregister 逻辑测试 ──

        [Fact]
        public void Unregister_注销所有Provider()
        {
            RimChatDetector.IsRimChatActive = true;
            var settings = BridgeRimChatSettings.Get();
            settings.enableContextPull = true;
            settings.pullDiplomacyHistory = true;
            settings.pullRpgHistory = true;

            ContextPullBridge.Register();
            Assert.Equal(2, RimMind.Presentation.RimMindAPI.ContextRegisterCount);

            ContextPullBridge.Unregister();
            Assert.Equal(2, RimMind.Presentation.RimMindAPI.ContextUnregisterCount);
        }

        // ── Refresh 逻辑测试 ──

        [Fact]
        public void Refresh_先注销再注册()
        {
            RimChatDetector.IsRimChatActive = true;
            var settings = BridgeRimChatSettings.Get();
            settings.enableContextPull = true;
            settings.pullDiplomacyHistory = true;
            settings.pullRpgHistory = false;

            ContextPullBridge.Register();
            Assert.Equal(1, RimMind.Presentation.RimMindAPI.ContextRegisterCount);

            RimMind.Presentation.RimMindAPI.ResetCounts();

            // Refresh = Unregister + Register
            ContextPullBridge.Refresh();
            Assert.Equal(1, RimMind.Presentation.RimMindAPI.ContextUnregisterCount);
            Assert.Equal(1, RimMind.Presentation.RimMindAPI.ContextRegisterCount);
        }

        /// <summary>
        /// 通过反射获取 ContextPullBridge.Truncate 方法
        /// </summary>
        private static MethodInfo GetTruncateMethod()
        {
            var method = typeof(ContextPullBridge).GetMethod("Truncate",
                BindingFlags.NonPublic | BindingFlags.Static);
            Assert.NotNull(method);
            return method!;
        }
    }
}
