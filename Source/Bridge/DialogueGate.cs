using RimMind.Bridge.RimChat.Detection;
using RimMind.Bridge.RimChat.Settings;
using Verse;

namespace RimMind.Bridge.RimChat.Bridge
{
    public static class DialogueGate
    {
        public static bool ShouldSkipDialogue(Pawn pawn, string triggerType)
        {
            if (!RimChatDetector.IsRimChatActive) return false;

            var settings = BridgeRimChatSettings.Get();

            return triggerType switch
            {
                "Chitchat" => settings.enableChitchatGate,
                "Auto" => settings.enableAutoGate,
                "PlayerInput" => ShouldSkipPlayerInput(settings),
                _ => false
            };
        }

        public static bool ShouldSkipFloatMenuOption()
        {
            if (!RimChatDetector.IsRimChatActive) return false;
            return ShouldSkipPlayerInput(BridgeRimChatSettings.Get());
        }

        private static bool ShouldSkipPlayerInput(BridgeRimChatSettings settings)
            => settings.enablePlayerInputGate
                && settings.skipPlayerDialogue
                && !settings.forceRimMindPlayerDialogue;
    }
}
