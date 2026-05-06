using RimMind.Bridge.RimChat.Bridge;
using RimMind.Bridge.RimChat.Detection;
using RimMind.Bridge.RimChat.Settings;
using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace RimMind.Bridge.RimChat.Tests
{
    public class RimChatDialogueGateTests
    {
        public RimChatDialogueGateTests()
        {
            RimChatDetector.IsRimChatActive = false;
            BridgeRimChatSettings.Reset();
        }

        [Fact]
        public void ShouldSkipDialogue_RimChatInactive_ReturnsFalse()
        {
            RimChatDetector.IsRimChatActive = false;
            Assert.False(DialogueGate.ShouldSkipDialogue(null!, "Chitchat"));
        }

        [Fact]
        public void ShouldSkipDialogue_ChibatEnabled_Skips()
        {
            RimChatDetector.IsRimChatActive = true;
            var settings = BridgeRimChatSettings.Get();
            settings.enableChitchatGate = true;
            Assert.True(DialogueGate.ShouldSkipDialogue(null!, "Chitchat"));
        }

        [Fact]
        public void ShouldSkipDialogue_ChibatDisabled_DoesNotSkip()
        {
            RimChatDetector.IsRimChatActive = true;
            var settings = BridgeRimChatSettings.Get();
            settings.enableChitchatGate = false;
            Assert.False(DialogueGate.ShouldSkipDialogue(null!, "Chitchat"));
        }

        [Fact]
        public void ShouldSkipDialogue_AutoEnabled_Skips()
        {
            RimChatDetector.IsRimChatActive = true;
            var settings = BridgeRimChatSettings.Get();
            settings.enableAutoGate = true;
            Assert.True(DialogueGate.ShouldSkipDialogue(null!, "Auto"));
        }

        [Fact]
        public void ShouldSkipDialogue_PlayerInput_AllConditionsMet_Skips()
        {
            RimChatDetector.IsRimChatActive = true;
            var settings = BridgeRimChatSettings.Get();
            settings.enablePlayerInputGate = true;
            settings.skipPlayerDialogue = true;
            settings.forceRimMindPlayerDialogue = false;
            Assert.True(DialogueGate.ShouldSkipDialogue(null!, "PlayerInput"));
        }

        [Fact]
        public void ShouldSkipDialogue_PlayerInput_GateDisabled_DoesNotSkip()
        {
            RimChatDetector.IsRimChatActive = true;
            var settings = BridgeRimChatSettings.Get();
            settings.enablePlayerInputGate = false;
            Assert.False(DialogueGate.ShouldSkipDialogue(null!, "PlayerInput"));
        }

        [Fact]
        public void ShouldSkipDialogue_PlayerInput_ForceRimMind_DoesNotSkip()
        {
            RimChatDetector.IsRimChatActive = true;
            var settings = BridgeRimChatSettings.Get();
            settings.enablePlayerInputGate = true;
            settings.skipPlayerDialogue = true;
            settings.forceRimMindPlayerDialogue = true;
            Assert.False(DialogueGate.ShouldSkipDialogue(null!, "PlayerInput"));
        }

        [Fact]
        public void ShouldSkipDialogue_UnknownTrigger_DoesNotSkip()
        {
            RimChatDetector.IsRimChatActive = true;
            Assert.False(DialogueGate.ShouldSkipDialogue(null!, "Unknown"));
        }

        [Fact]
        public void ShouldSkipFloatMenuOption_AllConditionsMet_Skips()
        {
            RimChatDetector.IsRimChatActive = true;
            var settings = BridgeRimChatSettings.Get();
            settings.enablePlayerInputGate = true;
            settings.skipPlayerDialogue = true;
            settings.forceRimMindPlayerDialogue = false;
            Assert.True(DialogueGate.ShouldSkipFloatMenuOption());
        }

        [Fact]
        public void ShouldSkipFloatMenuOption_RimChatInactive_DoesNotSkip()
        {
            RimChatDetector.IsRimChatActive = false;
            Assert.False(DialogueGate.ShouldSkipFloatMenuOption());
        }

        [Fact]
        public void ShouldSkipFloatMenuOption_ForceRimMind_DoesNotSkip()
        {
            RimChatDetector.IsRimChatActive = true;
            var settings = BridgeRimChatSettings.Get();
            settings.enablePlayerInputGate = true;
            settings.skipPlayerDialogue = true;
            settings.forceRimMindPlayerDialogue = true;
            Assert.False(DialogueGate.ShouldSkipFloatMenuOption());
        }
    }
}
