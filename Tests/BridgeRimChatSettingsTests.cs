using RimMind.Bridge.RimChat.Bridge;
using RimMind.Bridge.RimChat.Detection;
using RimMind.Bridge.RimChat.Settings;
using Xunit;

namespace RimMind.Bridge.RimChat.Tests
{
    /// <summary>
    /// BridgeRimChatSettings 默认值、单例模式、实时效果单元测试
    /// </summary>
    public class BridgeRimChatSettingsTests
    {
        public BridgeRimChatSettingsTests()
        {
            RimChatDetector.IsRimChatActive = false;
            BridgeRimChatSettings.Reset();
        }

        [Fact]
        public void 默认值_对话门控设置正确()
        {
            var settings = BridgeRimChatSettings.Get();
            Assert.True(settings.enablePlayerInputGate);
            Assert.True(settings.enableChitchatGate);
            Assert.True(settings.enableAutoGate);
            Assert.True(settings.skipPlayerDialogue);
            Assert.False(settings.forceRimMindPlayerDialogue);
        }

        [Fact]
        public void 默认值_动作门控设置正确()
        {
            var settings = BridgeRimChatSettings.Get();
            Assert.True(settings.enableActionGate);
            Assert.True(settings.skipDiplomacyActions);
            Assert.True(settings.skipTriggerIncident);
            Assert.False(settings.skipSocialActions);
            Assert.False(settings.skipRecruitAgree);
            Assert.Equal(60000, settings.incidentCooldownTicks);
            Assert.False(settings.forceRimMindActions);
        }

        [Fact]
        public void 默认值_上下文拉取设置正确()
        {
            var settings = BridgeRimChatSettings.Get();
            Assert.True(settings.enableContextPull);
            Assert.True(settings.pullDiplomacyHistory);
            Assert.False(settings.pullRpgHistory);
        }

        [Fact]
        public void Get_多次调用返回同一实例()
        {
            var first = BridgeRimChatSettings.Get();
            var second = BridgeRimChatSettings.Get();
            Assert.Same(first, second);
        }

        [Fact]
        public void Reset_清除单例_下次Get返回新实例()
        {
            var first = BridgeRimChatSettings.Get();
            first.enableChitchatGate = false;
            BridgeRimChatSettings.Reset();
            var second = BridgeRimChatSettings.Get();
            Assert.NotSame(first, second);
            Assert.True(second.enableChitchatGate);
        }

        [Fact]
        public void 构造函数_设置_instance()
        {
            BridgeRimChatSettings.Reset();
            var settings = new BridgeRimChatSettings();
            Assert.Same(settings, BridgeRimChatSettings.Get());
        }

        [Fact]
        public void 设置变更_实时影响DialogueGate()
        {
            RimChatDetector.IsRimChatActive = true;
            var settings = BridgeRimChatSettings.Get();

            // 初始状态：Chitchat门控开启，应跳过
            settings.enableChitchatGate = true;
            Assert.True(DialogueGate.ShouldSkipDialogue(null!, "Chitchat"));

            // 修改设置后：Chitchat门控关闭，不应跳过
            settings.enableChitchatGate = false;
            Assert.False(DialogueGate.ShouldSkipDialogue(null!, "Chitchat"));
        }

        [Fact]
        public void 设置变更_实时影响ActionGate()
        {
            RimChatDetector.IsRimChatActive = true;
            var settings = BridgeRimChatSettings.Get();

            // 初始状态：外交动作跳过开启
            settings.skipDiplomacyActions = true;
            Assert.True(ActionGate.ShouldSkipAction("adjust_faction"));

            // 修改设置后：外交动作跳过关闭
            settings.skipDiplomacyActions = false;
            Assert.False(ActionGate.ShouldSkipAction("adjust_faction"));
        }

        [Fact]
        public void 设置变更_forceRimMindActions_实时影响ActionGate()
        {
            RimChatDetector.IsRimChatActive = true;
            var settings = BridgeRimChatSettings.Get();

            // 开启forceRimMindActions后，即使skipDiplomacyActions为true也不跳过
            settings.skipDiplomacyActions = true;
            settings.forceRimMindActions = true;
            Assert.False(ActionGate.ShouldSkipAction("adjust_faction"));

            // 关闭forceRimMindActions后恢复跳过
            settings.forceRimMindActions = false;
            Assert.True(ActionGate.ShouldSkipAction("adjust_faction"));
        }

        [Fact]
        public void 设置变更_forceRimMindPlayerDialogue_实时影响FloatMenu()
        {
            RimChatDetector.IsRimChatActive = true;
            var settings = BridgeRimChatSettings.Get();

            settings.enablePlayerInputGate = true;
            settings.skipPlayerDialogue = true;
            settings.forceRimMindPlayerDialogue = false;
            Assert.True(DialogueGate.ShouldSkipFloatMenuOption());

            // 开启forceRimMindPlayerDialogue后不跳过
            settings.forceRimMindPlayerDialogue = true;
            Assert.False(DialogueGate.ShouldSkipFloatMenuOption());
        }
    }
}
