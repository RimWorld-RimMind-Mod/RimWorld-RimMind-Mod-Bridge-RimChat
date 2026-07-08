using System.Reflection;
using RimMind.Bridge.RimChat.Bridge;
using RimMind.Bridge.RimChat;
using Xunit;

namespace RimMind.Bridge.RimChat.Tests
{
    /// <summary>
    /// 验证所有 Extension 和 ContextPullBridge 使用统一的 OwnerModId/ModId。
    /// About.xml packageId 为 mcocdaa.RimMindBridgeRimChat，代码中统一用 "RimMindBridgeRimChat"。
    /// Core 的 UnregisterByOwner(modId) 精确字符串匹配，不一致会导致卸载残留。
    /// </summary>
    public class OwnerModIdConsistencyTests
    {
        private const string ExpectedModId = "RimMindBridgeRimChat";

        [Fact]
        public void RimChatIncidentExecutedListener_OwnerModId_一致()
        {
            var listener = new RimChatIncidentExecutedListener();
            Assert.Equal(ExpectedModId, listener.OwnerModId);
        }

        [Fact]
        public void RimChatActionSkipCheck_OwnerModId_一致()
        {
            var check = new RimChatActionSkipCheck();
            Assert.Equal(ExpectedModId, check.OwnerModId);
        }

        [Fact]
        public void RimChatDialogueSkipCheck_OwnerModId_一致()
        {
            // RimChatDialogueSkipCheck 构造函数已改为无参（_mod 字段已作为死代码删除）。
            var check = new RimChatDialogueSkipCheck();
            Assert.Equal(ExpectedModId, check.OwnerModId);
        }

        [Fact]
        public void RimChatFloatMenuSkipCheck_OwnerModId_一致()
        {
            var check = new RimChatFloatMenuSkipCheck();
            Assert.Equal(ExpectedModId, check.OwnerModId);
        }

        [Fact]
        public void RimChatStorytellerIncidentSkipCheck_OwnerModId_一致()
        {
            var check = new RimChatStorytellerIncidentSkipCheck();
            Assert.Equal(ExpectedModId, check.OwnerModId);
        }

        [Fact]
        public void RimChatSettingsTab_OwnerModId_一致()
        {
            var tab = new RimChatSettingsTab();
            Assert.Equal(ExpectedModId, tab.OwnerModId);
        }

        [Fact]
        public void ContextPullBridge_ModId_一致()
        {
            // ContextPullBridge.ModId 为 private const，通过反射读取。
            // 该值作为 ContextProviderDef.OwnerMod 注册到 Core，必须与 Extension 的 OwnerModId 一致。
            var field = typeof(ContextPullBridge).GetField("ModId",
                BindingFlags.NonPublic | BindingFlags.Static);
            Assert.NotNull(field);
            Assert.Equal(ExpectedModId, (string?)field!.GetValue(null));
        }
    }
}
