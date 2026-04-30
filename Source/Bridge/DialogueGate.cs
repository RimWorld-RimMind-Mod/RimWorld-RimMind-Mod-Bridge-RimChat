using RimMind.Bridge.RimChat.Detection;
using RimMind.Bridge.RimChat.Settings;
using RimMind.Core;
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
                "PlayerInput" => settings.enablePlayerInputGate
                    && settings.skipPlayerDialogue
                    && !settings.forceRimMindPlayerDialogue,
                _ => false
            };
        }

        public static bool ShouldSkipFloatMenuOption()
        {
            if (!RimChatDetector.IsRimChatActive) return false;

            var settings = BridgeRimChatSettings.Get();

            return settings.enablePlayerInputGate
                && settings.skipPlayerDialogue
                && !settings.forceRimMindPlayerDialogue;
        }

        internal static void RegisterSkipChecks()
        {
            RimMindAPI.RegisterDialogueSkipCheck("rimchat_bridge", ShouldSkipDialogue);
            RimMindAPI.RegisterFloatMenuSkipCheck("rimchat_bridge", ShouldSkipFloatMenuOption);
        }

    }
}
