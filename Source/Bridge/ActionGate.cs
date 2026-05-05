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
            "romance_attempt",
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

        private static string? _incidentCallbackKey;
        private static string? _storytellerSkipCheckKey;

        internal static void Register()
        {
            RimMindAPI.RegisterActionSkipCheck("rimchat_bridge", ShouldSkipAction);
            _incidentCallbackKey = RimMindAPI.RegisterIncidentExecutedCallback(SharedIncidentCooldown.RecordIncident);
            _storytellerSkipCheckKey = RimMindAPI.RegisterStorytellerIncidentSkipCheck(ShouldSkipStorytellerIncident);
        }

        internal static void Unregister()
        {
            if (_incidentCallbackKey != null)
            {
                RimMindAPI.UnregisterIncidentExecutedCallback(_incidentCallbackKey);
                _incidentCallbackKey = null;
            }
            if (_storytellerSkipCheckKey != null)
            {
                RimMindAPI.UnregisterStorytellerIncidentSkipCheck(_storytellerSkipCheckKey);
                _storytellerSkipCheckKey = null;
            }
        }

    }
}
