using RimMind.Bridge.RimChat.Bridge;
using RimMind.Bridge.RimChat.Cooldown;
using RimMind.Bridge.RimChat.Detection;
using RimMind.Bridge.RimChat.Settings;
using Xunit;

namespace RimMind.Bridge.RimChat.Tests
{
    [Collection("RimChat")]
    public class ActionGateTests
    {
        public ActionGateTests()
        {
            RimChatDetector.IsRimChatActive = false;
            BridgeRimChatSettings.Reset();
        }

        [Fact]
        public void ShouldSkipAction_RimChatInactive_ReturnsFalse()
        {
            RimChatDetector.IsRimChatActive = false;
            Assert.False(ActionGate.ShouldSkipAction("adjust_faction"));
        }

        [Fact]
        public void ShouldSkipAction_GateDisabled_ReturnsFalse()
        {
            RimChatDetector.IsRimChatActive = true;
            var settings = BridgeRimChatSettings.Get();
            settings.enableActionGate = false;
            Assert.False(ActionGate.ShouldSkipAction("adjust_faction"));
        }

        [Fact]
        public void ShouldSkipAction_ForceRimMind_ReturnsFalse()
        {
            RimChatDetector.IsRimChatActive = true;
            var settings = BridgeRimChatSettings.Get();
            settings.forceRimMindActions = true;
            Assert.False(ActionGate.ShouldSkipAction("adjust_faction"));
        }

        [Fact]
        public void ShouldSkipAction_Diplomacy_Skips()
        {
            RimChatDetector.IsRimChatActive = true;
            var settings = BridgeRimChatSettings.Get();
            settings.skipDiplomacyActions = true;
            Assert.True(ActionGate.ShouldSkipAction("adjust_faction"));
            Assert.True(ActionGate.ShouldSkipAction("trigger_incident"));
        }

        [Fact]
        public void ShouldSkipAction_Social_SkipsWhenEnabled()
        {
            RimChatDetector.IsRimChatActive = true;
            var settings = BridgeRimChatSettings.Get();
            settings.skipSocialActions = true;
            Assert.True(ActionGate.ShouldSkipAction("romance_attempt"));
            Assert.True(ActionGate.ShouldSkipAction("romance_breakup"));
        }

        [Fact]
        public void ShouldSkipAction_Social_DoesNotSkipWhenDisabled()
        {
            RimChatDetector.IsRimChatActive = true;
            var settings = BridgeRimChatSettings.Get();
            settings.skipSocialActions = false;
            Assert.False(ActionGate.ShouldSkipAction("romance_attempt"));
        }

        [Fact]
        public void ShouldSkipAction_RecruitAgree_SkipsWhenEnabled()
        {
            RimChatDetector.IsRimChatActive = true;
            var settings = BridgeRimChatSettings.Get();
            settings.skipRecruitAgree = true;
            Assert.True(ActionGate.ShouldSkipAction("recruit_agree"));
        }

        [Fact]
        public void ShouldSkipAction_UnknownIntent_DoesNotSkip()
        {
            RimChatDetector.IsRimChatActive = true;
            Assert.False(ActionGate.ShouldSkipAction("unknown_action"));
        }

        [Fact]
        public void ShouldSkipStorytellerIncident_RimChatInactive_ReturnsFalse()
        {
            RimChatDetector.IsRimChatActive = false;
            Assert.False(ActionGate.ShouldSkipStorytellerIncident());
        }

        [Fact]
        public void ShouldSkipStorytellerIncident_OnCooldown_ReturnsTrue()
        {
            RimChatDetector.IsRimChatActive = true;
            var settings = BridgeRimChatSettings.Get();
            settings.enableActionGate = true;
            settings.skipTriggerIncident = true;
            settings.incidentCooldownTicks = 60000;
            SharedIncidentCooldown.RecordIncident();
            Assert.True(ActionGate.ShouldSkipStorytellerIncident());
        }
    }
}
