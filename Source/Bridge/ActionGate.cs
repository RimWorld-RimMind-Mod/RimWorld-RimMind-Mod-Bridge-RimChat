using System.Collections.Generic;
using RimMind.Bridge.RimChat.Cooldown;
using RimMind.Bridge.RimChat.Detection;
using RimMind.Bridge.RimChat.Settings;
using RimMind.Core;
using Verse;

namespace RimMind.Bridge.RimChat.Bridge
{
    public static class ActionGate
    {
        private static readonly HashSet<string> DiplomacyActions = new HashSet<string>
        {
            "adjust_faction",
            "trigger_incident",
        };

        private static readonly HashSet<string> SocialActions = new HashSet<string>
        {
            "romance_accept",
            "romance_breakup",
        };

        public static bool ShouldSkipAction(string intentId)
        {
            if (!RimChatDetector.IsRimChatActive) return false;

            var settings = BridgeRimChatSettings.Get();
            if (!settings.enableActionGate) return false;
            if (settings.forceRimMindActions) return false;

            if (settings.skipDiplomacyActions && DiplomacyActions.Contains(intentId))
                return true;

            if (settings.skipSocialActions && SocialActions.Contains(intentId))
                return true;

            if (settings.skipRecruitAgree && intentId == "recruit_agree")
                return true;

            return false;
        }

        public static bool ShouldSkipStorytellerIncident()
        {
            if (!RimChatDetector.IsRimChatActive) return false;

            var settings = BridgeRimChatSettings.Get();
            if (!settings.enableActionGate) return false;
            if (!settings.skipTriggerIncident) return false;

            return SharedIncidentCooldown.IsOnCooldown(settings.incidentCooldownTicks);
        }

        internal static void Register()
        {
            RimMindAPI.RegisterActionSkipCheck("rimchat_bridge", ShouldSkipAction);
            RimMindAPI.RegisterIncidentExecutedCallback(SharedIncidentCooldown.RecordIncident);
            RimMindAPI.RegisterStorytellerIncidentSkipCheck(ShouldSkipStorytellerIncident);
        }

        internal static void Unregister()
        {
            RimMindAPI.UnregisterActionSkipCheck("rimchat_bridge");
            RimMindAPI.UnregisterIncidentExecutedCallback(SharedIncidentCooldown.RecordIncident);
            RimMindAPI.UnregisterStorytellerIncidentSkipCheck(ShouldSkipStorytellerIncident);
        }
    }
}
