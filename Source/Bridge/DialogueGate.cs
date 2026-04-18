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
            if (!settings.enableDialogueGate) return false;

            if (triggerType == "PlayerInput" && settings.skipPlayerDialogue)
                return !settings.forceRimMindPlayerDialogue;

            return false;
        }

        public static bool ShouldSkipFloatMenuOption()
        {
            if (!RimChatDetector.IsRimChatActive) return false;

            var settings = BridgeRimChatSettings.Get();
            if (!settings.enableDialogueGate) return false;

            return settings.skipPlayerDialogue && !settings.forceRimMindPlayerDialogue;
        }

        internal static void RegisterSkipChecks()
        {
            RimMindAPI.RegisterDialogueSkipCheck("rimchat_bridge", ShouldSkipDialogue);
            RimMindAPI.RegisterFloatMenuSkipCheck("rimchat_bridge", ShouldSkipFloatMenuOption);
        }

        internal static void UnregisterSkipChecks()
        {
            RimMindAPI.UnregisterDialogueSkipCheck("rimchat_bridge");
            RimMindAPI.UnregisterFloatMenuSkipCheck("rimchat_bridge");
        }
    }
}
