using System.Reflection;
using RimMind.Bridge.RimChat.Bridge;
using RimMind.Bridge.RimChat.Detection;
using RimMind.Bridge.RimChat.Settings;
using Xunit;

namespace RimMind.Bridge.RimChat.Tests
{
    /// <summary>
    /// 验证 ActionGate 所有动作分类都使用 HashSet 模式，
    /// 不存在 == 字符串比较的混合写法。
    /// </summary>
    public class ActionGateClassificationTests
    {
        public ActionGateClassificationTests()
        {
            RimChatDetector.IsRimChatActive = false;
            BridgeRimChatSettings.Reset();
        }

        [Fact]
        public void RecruitActions_应存在为HashSet()
        {
            var field = typeof(ActionGate).GetField("RecruitActions",
                BindingFlags.NonPublic | BindingFlags.Static);
            Assert.NotNull(field);
            var set = field?.GetValue(null) as System.Collections.Generic.HashSet<string>;
            Assert.NotNull(set);
            Assert.Contains("recruit_agree", set);
        }

        [Fact]
        public void ShouldSkipAction_recruit_agree_在RecruitActions开启时跳过()
        {
            RimChatDetector.IsRimChatActive = true;
            var s = BridgeRimChatSettings.Get();
            s.enableActionGate = true;
            s.skipRecruitAgree = true;
            Assert.True(ActionGate.ShouldSkipAction("recruit_agree"));
        }

        [Fact]
        public void ShouldSkipAction_recruit_agree_在RecruitActions关闭时不跳过()
        {
            RimChatDetector.IsRimChatActive = true;
            var s = BridgeRimChatSettings.Get();
            s.enableActionGate = true;
            s.skipRecruitAgree = false;
            Assert.False(ActionGate.ShouldSkipAction("recruit_agree"));
        }
    }
}
