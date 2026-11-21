using System.Linq;
using System.Text;
using LudeonTK;
using RimMind.Bridge.RimChat.Bridge;
using RimMind.Bridge.RimChat.Detection;
using RimMind.Bridge.RimChat.Settings;
using RimMind.Domain.ValueObjects;
using RimMind.Presentation.Api;
using Verse;

namespace RimMind.Bridge.RimChat.Debug
{
    [StaticConstructorOnStartup]
    public static class BridgeRimChatDebugActions
    {
        [DebugAction("RimMind Bridge-RimChat", "Show Bridge State",
            actionType = DebugActionType.Action)]
        public static void ShowBridgeState()
        {
            var settings = BridgeRimChatSettings.Get();
            var sb = new StringBuilder("[RimMind-Bridge-RimChat] Bridge State:\n");
            sb.AppendLine($"  RimChatDetector.IsRimChatActive: {RimChatDetector.IsRimChatActive}");
            sb.AppendLine();
            sb.AppendLine("  DialogueGate:");
            sb.AppendLine($"    enablePlayerInputGate: {settings.enablePlayerInputGate}");
            sb.AppendLine($"    enableChitchatGate: {settings.enableChitchatGate}");
            sb.AppendLine($"    enableAutoGate: {settings.enableAutoGate}");
            sb.AppendLine($"    skipPlayerDialogue: {settings.skipPlayerDialogue}");
            sb.AppendLine($"    forceRimMindPlayerDialogue: {settings.forceRimMindPlayerDialogue}");
            sb.AppendLine();
            sb.AppendLine("  ActionGate:");
            sb.AppendLine($"    enableActionGate: {settings.enableActionGate}");
            sb.AppendLine($"    skipDiplomacyActions: {settings.skipDiplomacyActions}");
            sb.AppendLine($"    skipTriggerIncident: {settings.skipTriggerIncident}");
            sb.AppendLine($"    skipSocialActions: {settings.skipSocialActions}");
            sb.AppendLine($"    skipRecruitAgree: {settings.skipRecruitAgree}");
            sb.AppendLine($"    forceRimMindActions: {settings.forceRimMindActions}");
            sb.AppendLine($"    incidentCooldownTicks: {settings.incidentCooldownTicks}");
            sb.AppendLine();
            sb.AppendLine("  ContextPull:");
            sb.AppendLine($"    enableContextPull: {settings.enableContextPull}");
            sb.AppendLine($"    pullDiplomacyHistory: {settings.pullDiplomacyHistory}");
            sb.AppendLine($"    pullRpgHistory: {settings.pullRpgHistory}");
            Log.Message(sb.ToString());
        }

        [DebugAction("RimMind Bridge-RimChat", "Test Dialogue Gate (selected)",
            actionType = DebugActionType.Action)]
        public static void TestDialogueGate()
        {
            var pawn = Find.Selector.SingleSelectedThing as Pawn;
            if (pawn == null)
            {
                RimMindErrors.Warn("[RimMind-Bridge-RimChat] Please select a pawn on the map first.");
                return;
            }
            var sb = new StringBuilder($"[RimMind-Bridge-RimChat] Dialogue Gate Test ({pawn.Name.ToStringShort}):\n");
            sb.AppendLine($"  ShouldSkipDialogue(\"Chitchat\"): {RimMindAPI.Ext.ShouldSkipDialogue(pawn, "Chitchat")}");
            sb.AppendLine($"  ShouldSkipDialogue(\"Auto\"): {RimMindAPI.Ext.ShouldSkipDialogue(pawn, "Auto")}");
            sb.AppendLine($"  ShouldSkipDialogue(\"PlayerInput\"): {RimMindAPI.Ext.ShouldSkipDialogue(pawn, "PlayerInput")}");
            sb.AppendLine($"  ShouldSkipFloatMenu(): {RimMindAPI.Ext.ShouldSkipFloatMenu()}");
            Log.Message(sb.ToString());
        }

        [DebugAction("RimMind Bridge-RimChat", "Test Action Gate",
            actionType = DebugActionType.Action)]
        public static void TestActionGate()
        {
            var sb = new StringBuilder("[RimMind-Bridge-RimChat] Action Gate Test:\n");
            sb.AppendLine($"  ShouldSkipAction(\"adjust_faction\"): {RimMindAPI.Ext.ShouldSkipAction("adjust_faction")}");
            sb.AppendLine($"  ShouldSkipAction(\"trigger_incident\"): {RimMindAPI.Ext.ShouldSkipAction("trigger_incident")}");
            sb.AppendLine($"  ShouldSkipAction(\"romance_attempt\"): {RimMindAPI.Ext.ShouldSkipAction("romance_attempt")}");
            sb.AppendLine($"  ShouldSkipAction(\"recruit_agree\"): {RimMindAPI.Ext.ShouldSkipAction("recruit_agree")}");
            sb.AppendLine($"  ShouldSkipStorytellerIncident(): {RimMindAPI.Ext.ShouldSkipStorytellerIncident()}");
            Log.Message(sb.ToString());
        }

        [DebugAction("RimMind Bridge-RimChat", "Show Context Pull State",
            actionType = DebugActionType.Action)]
        public static void ShowContextPullState()
        {
            var settings = BridgeRimChatSettings.Get();
            var categories = RimMindAPI.Providers.GetRegisteredCategories();
            var rimchatCategories = categories.Where(c => c.StartsWith("rimchat_")).ToList();
            var sb = new StringBuilder("[RimMind-Bridge-RimChat] Context Pull State:\n");
            sb.AppendLine($"  Registered rimchat categories ({rimchatCategories.Count}):");
            if (rimchatCategories.Count == 0)
                sb.AppendLine("    (none)");
            else
                foreach (var cat in rimchatCategories)
                    sb.AppendLine($"    {cat}");
            sb.AppendLine();
            sb.AppendLine("  Settings:");
            sb.AppendLine($"    pullDiplomacyHistory: {settings.pullDiplomacyHistory}");
            sb.AppendLine($"    pullRpgHistory: {settings.pullRpgHistory}");
            Log.Message(sb.ToString());
        }

        [DebugAction("RimMind Bridge-RimChat", "Force Unregister Bridges",
            actionType = DebugActionType.Action)]
        public static void ForceUnregisterBridges()
        {
            RimChatBridgeCoordinator.Unregister();
            Log.Message("[RimMind-Bridge-RimChat] All bridges unregistered.");
        }
    }
}
