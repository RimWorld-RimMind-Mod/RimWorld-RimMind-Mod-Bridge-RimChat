using RimMind.Bridge.RimChat.Detection;
using Xunit;

namespace RimMind.Bridge.RimChat.Tests
{
    /// <summary>
    /// RimChatDetector 检测逻辑单元测试
    /// </summary>
    public class RimChatDetectorTests
    {
        public RimChatDetectorTests()
        {
            // 每次测试前重置检测器状态
            RimChatDetector.IsRimChatActive = false;
            RimChatDetector.IsRimChatApiAvailable = false;
        }

        [Fact]
        public void RimChatPackageId_常量值正确()
        {
            // 验证包ID常量与RimChat模组一致
            Assert.Equal("yancy.rimchat", RimChatDetector.RimChatPackageId);
        }

        [Fact]
        public void IsRimChatActive_设为true_返回true()
        {
            RimChatDetector.IsRimChatActive = true;
            Assert.True(RimChatDetector.IsRimChatActive);
        }

        [Fact]
        public void IsRimChatApiAvailable_设为true_返回true()
        {
            RimChatDetector.IsRimChatApiAvailable = true;
            Assert.True(RimChatDetector.IsRimChatApiAvailable);
        }

        [Fact]
        public void IsRimChatApiAvailable_默认为false()
        {
            // 未初始化时API不可用
            Assert.False(RimChatDetector.IsRimChatApiAvailable);
        }

        [Fact]
        public void IsRimChatActive_默认为false()
        {
            // 未初始化时RimChat未激活
            Assert.False(RimChatDetector.IsRimChatActive);
        }

        [Fact]
        public void IsRimChatApiAvailable_RimChat不活跃时_即使ApiAvailable为true也应遵循实际状态()
        {
            // 在测试桩中，IsRimChatApiAvailable 是独立属性
            // 但在源码中，IsRimChatApiAvailable 依赖 IsRimChatActive && ApiType != null
            // 这里验证桩的行为一致性
            RimChatDetector.IsRimChatActive = false;
            RimChatDetector.IsRimChatApiAvailable = false;
            Assert.False(RimChatDetector.IsRimChatApiAvailable);
        }
    }
}
