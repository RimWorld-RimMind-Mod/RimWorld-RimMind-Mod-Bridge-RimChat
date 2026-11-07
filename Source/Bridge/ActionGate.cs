using System;
using System.Collections.Generic;
using RimMind.Bridge.RimChat.Cooldown;
using RimMind.Bridge.RimChat.Detection;
using RimMind.Bridge.RimChat.Settings;
using Verse;

namespace RimMind.Bridge.RimChat.Bridge
{
    public static class ActionGate
    {
        private static readonly HashSet<string> DiplomacyActions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "adjust_faction",
            "trigger_incident",
            "world.faction",
            "diplomacy",
            "world.storyteller",
            "storyteller.incident",
        };

        private static readonly HashSet<string> SocialActions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "romance_attempt",
            "romance_breakup",
            "pawn.interaction",
            "social_relax",
            "give_item",
            "actions_social_interact",
            "social_interact",
        };

        private static readonly HashSet<string> RecruitActions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "recruit_agree",
            "recruit",
            "actions_recruit",
            "pawn.recruit",
            "prisoner.recruit",
        };

        public static bool ShouldSkipAction(string? intentId)
        {
            if (string.IsNullOrWhiteSpace(intentId)) return false;
            if (!RimChatDetector.IsRimChatActive) return false;

            var settings = BridgeRimChatSettings.Get();
            if (!settings.enableActionGate) return false;
            if (settings.forceRimMindActions) return false;

            if (settings.skipDiplomacyActions && IsDiplomacyAction(intentId))
                return true;

            if (settings.skipSocialActions && IsSocialAction(intentId))
                return true;

            if (settings.skipRecruitAgree && IsRecruitAction(intentId))
                return true;

            return false;
        }

        public static bool IsRomanceAction(string? intentId)
        {
            if (string.IsNullOrWhiteSpace(intentId)) return false;
            string normalized = intentId!.Trim();
            if (normalized.StartsWith("romance_", StringComparison.OrdinalIgnoreCase))
                return true;
            if (MatchesPrefix(normalized, "pawn.interaction")
                && (normalized.IndexOf("romance", StringComparison.OrdinalIgnoreCase) >= 0
                    || normalized.EndsWith(".romance_attempt", StringComparison.OrdinalIgnoreCase)
                    || normalized.EndsWith(".romance_breakup", StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }
            return normalized.IndexOf("romance", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public static bool IsSocialAction(string? intentId)
        {
            if (string.IsNullOrWhiteSpace(intentId)) return false;
            string normalized = intentId!.Trim();
            if (IsRomanceAction(normalized)) return true;
            if (SocialActions.Contains(normalized)) return true;
            return MatchesPrefix(normalized, "pawn.interaction")
                || normalized.StartsWith("pawn_interaction", StringComparison.OrdinalIgnoreCase)
                || normalized.StartsWith("social.", StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsDiplomacyAction(string? intentId)
        {
            if (string.IsNullOrWhiteSpace(intentId)) return false;
            string normalized = intentId!.Trim();
            if (DiplomacyActions.Contains(normalized)) return true;
            return MatchesPrefix(normalized, "world.faction")
                || normalized.StartsWith("faction.", StringComparison.OrdinalIgnoreCase)
                || normalized.StartsWith("faction_", StringComparison.OrdinalIgnoreCase)
                || MatchesPrefix(normalized, "diplomacy")
                || normalized.IndexOf("diplomacy", StringComparison.OrdinalIgnoreCase) >= 0
                || MatchesPrefix(normalized, "world.storyteller")
                || MatchesPrefix(normalized, "storyteller.incident")
                || normalized.StartsWith("incident.", StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsRecruitAction(string? intentId)
        {
            if (string.IsNullOrWhiteSpace(intentId)) return false;
            string normalized = intentId!.Trim();
            if (RecruitActions.Contains(normalized)) return true;
            return MatchesPrefix(normalized, "recruit")
                || normalized.StartsWith("recruit_", StringComparison.OrdinalIgnoreCase)
                || MatchesPrefix(normalized, "pawn.recruit")
                || MatchesPrefix(normalized, "prisoner.recruit")
                || normalized.IndexOf("recruit", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static bool MatchesPrefix(string value, string prefix)
        {
            if (string.Equals(value, prefix, StringComparison.OrdinalIgnoreCase))
                return true;
            return value.Length > prefix.Length
                && value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
                && (value[prefix.Length] == '.' || value[prefix.Length] == ':' || value[prefix.Length] == '_');
        }

        public static bool ShouldSkipStorytellerIncident()
        {
            if (!RimChatDetector.IsRimChatActive) return false;

            var settings = BridgeRimChatSettings.Get();
            if (!settings.enableActionGate) return false;
            if (!settings.skipTriggerIncident) return false;

            return SharedIncidentCooldown.IsOnCooldown(settings.incidentCooldownTicks);
        }
    }
}
