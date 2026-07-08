using System.Reflection;
using RimMind.Bridge.RimChat.Settings;
using Xunit;

namespace RimMind.Bridge.RimChat.Tests
{
    /// <summary>
    /// 验证 BridgeRimChatSettings.ApplyDefaults() 方法存在，
    /// 且与字段初始化器默认值一致。
    /// </summary>
    public class BridgeRimChatSettingsDefaultsTests
    {
        public BridgeRimChatSettingsDefaultsTests()
        {
            BridgeRimChatSettings.Reset();
        }

        [Fact]
        public void ApplyDefaults_方法存在()
        {
            var method = typeof(BridgeRimChatSettings).GetMethod("ApplyDefaults",
                BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(method);
        }

        [Fact]
        public void ApplyDefaults_重置所有字段为默认值()
        {
            var s = BridgeRimChatSettings.Get();
            // 先改成非默认值
            s.enablePlayerInputGate = false;
            s.enableChitchatGate = false;
            s.enableAutoGate = false;
            s.skipPlayerDialogue = false;
            s.forceRimMindPlayerDialogue = true;
            s.enableActionGate = false;
            s.skipDiplomacyActions = false;
            s.skipTriggerIncident = false;
            s.skipSocialActions = true;
            s.skipRecruitAgree = true;
            s.incidentCooldownTicks = 99999;
            s.forceRimMindActions = true;
            s.enableContextPull = false;
            s.pullDiplomacyHistory = false;
            s.pullRpgHistory = true;

            s.ApplyDefaults();

            Assert.True(s.enablePlayerInputGate);
            Assert.True(s.enableChitchatGate);
            Assert.True(s.enableAutoGate);
            Assert.True(s.skipPlayerDialogue);
            Assert.False(s.forceRimMindPlayerDialogue);
            Assert.True(s.enableActionGate);
            Assert.True(s.skipDiplomacyActions);
            Assert.True(s.skipTriggerIncident);
            Assert.False(s.skipSocialActions);
            Assert.False(s.skipRecruitAgree);
            Assert.Equal(60000, s.incidentCooldownTicks);
            Assert.False(s.forceRimMindActions);
            Assert.True(s.enableContextPull);
            Assert.True(s.pullDiplomacyHistory);
            Assert.False(s.pullRpgHistory);
        }
    }
}
