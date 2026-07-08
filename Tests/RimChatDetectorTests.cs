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
            RimChatDetector.IsRimChatActive = false;
        }

        [Fact]
        public void RimChatPackageId_常量值正确()
        {
            Assert.Equal("yancy.rimchat", RimChatDetector.RimChatPackageId);
        }

        [Fact]
        public void IsRimChatActive_设为true_返回true()
        {
            RimChatDetector.IsRimChatActive = true;
            Assert.True(RimChatDetector.IsRimChatActive);
        }

        [Fact]
        public void IsRimChatActive_默认为false()
        {
            Assert.False(RimChatDetector.IsRimChatActive);
        }
    }
}
