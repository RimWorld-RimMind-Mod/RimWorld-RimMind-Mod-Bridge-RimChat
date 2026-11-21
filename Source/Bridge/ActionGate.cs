using System;
using RimMind.Application.Common.Models.Tools;
using RimMind.Application.Features.Tools;
using RimMind.Bridge.RimChat.Cooldown;
using RimMind.Bridge.RimChat.Detection;
using RimMind.Bridge.RimChat.Settings;
using RimMind.Domain.Agent.Policies;
using RimMind.Presentation.Api;

namespace RimMind.Bridge.RimChat.Bridge
{
    public static class ActionGate
    {
        private static readonly char[] TokenSeparators = { '.', '_', ':', '-', '/', ' ' };

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

            var def = ResolveToolDefinition(normalized);
            if (def != null)
            {
                var manifest = def.Manifest ?? ToolManifest.Default;
                if (manifest.Kind == ToolKind.Query)
                    return false;

                if (ContainsToken(manifest.AutonomyDomainId, "romance"))
                    return true;

                ToolScope resolvedScopes = ToolScopeProjection.ResolveScopes(def);
                bool hasExplicitScope = manifest.Scopes.HasValue && manifest.Scopes.Value != ToolScope.None;
                if (hasExplicitScope && (resolvedScopes & ToolScope.Social) == ToolScope.None)
                    return false;

                if ((resolvedScopes & ToolScope.Social) != ToolScope.None)
                    return MentionsToken(def, "romance");
            }

            return normalized.IndexOf("romance", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public static bool IsSocialAction(string? intentId)
        {
            if (string.IsNullOrWhiteSpace(intentId)) return false;
            string normalized = intentId!.Trim();

            var def = ResolveToolDefinition(normalized);
            if (def != null)
            {
                var manifest = def.Manifest ?? ToolManifest.Default;
                if (manifest.Kind == ToolKind.Query)
                    return false;

                ToolScope resolvedScopes = ToolScopeProjection.ResolveScopes(def);
                if (IsRecruitDefinition(def, manifest, resolvedScopes))
                    return false;

                if (ContainsToken(manifest.AutonomyDomainId, "social")
                    || ContainsToken(manifest.AutonomyDomainId, "romance"))
                {
                    return true;
                }

                if ((resolvedScopes & ToolScope.Social) != ToolScope.None)
                    return true;

                if (HasAuthoritativeNonMatchingMetadata(manifest))
                    return false;
            }

            // Minimal explicit fallback for unregistered or legacy intents lacking ToolManifest metadata.
            if (normalized.IndexOf("romance", StringComparison.OrdinalIgnoreCase) >= 0)
                return true;

            return MatchesPrefix(normalized, "pawn.interaction")
                || MatchesPrefix(normalized, "pawn_interaction")
                || ContainsToken(normalized, "social")
                || string.Equals(normalized, "give_item", StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsDiplomacyAction(string? intentId)
        {
            if (string.IsNullOrWhiteSpace(intentId)) return false;
            string normalized = intentId!.Trim();

            var def = ResolveToolDefinition(normalized);
            if (def != null)
            {
                var manifest = def.Manifest ?? ToolManifest.Default;
                if (manifest.Kind == ToolKind.Query)
                    return false;

                if (ContainsAnyToken(manifest.AutonomyDomainId, "diplomacy", "faction", "storyteller", "incident"))
                    return true;

                ToolScope resolvedScopes = ToolScopeProjection.ResolveScopes(def);
                if ((resolvedScopes & ToolScope.WorldAdmin) != ToolScope.None)
                    return true;

                if (HasAuthoritativeNonMatchingMetadata(manifest))
                    return false;
            }

            // Minimal explicit fallback for unregistered or legacy intents lacking ToolManifest metadata.
            return ContainsAnyToken(normalized, "diplomacy", "faction", "storyteller", "incident");
        }

        public static bool IsRecruitAction(string? intentId)
        {
            if (string.IsNullOrWhiteSpace(intentId)) return false;
            string normalized = intentId!.Trim();

            var def = ResolveToolDefinition(normalized);
            if (def != null)
            {
                var manifest = def.Manifest ?? ToolManifest.Default;
                if (manifest.Kind == ToolKind.Query)
                    return false;

                ToolScope resolvedScopes = ToolScopeProjection.ResolveScopes(def);
                if (IsRecruitDefinition(def, manifest, resolvedScopes))
                    return true;

                if (HasAuthoritativeNonMatchingMetadata(manifest))
                    return false;
            }

            // Minimal explicit fallback for unregistered or legacy intents lacking ToolManifest metadata.
            return ContainsToken(normalized, "recruit");
        }

        public static bool ShouldSkipStorytellerIncident()
        {
            if (!RimChatDetector.IsRimChatActive) return false;

            var settings = BridgeRimChatSettings.Get();
            if (!settings.enableActionGate) return false;
            if (!settings.skipTriggerIncident) return false;

            return SharedIncidentCooldown.IsOnCooldown(settings.incidentCooldownTicks);
        }

        private static ToolDefinition? ResolveToolDefinition(string intentId)
        {
            try
            {
                return RimMindAPI.ToolSet.Registry?.FindById(intentId)?.Definition;
            }
            catch
            {
                // Runtime/ToolRegistry may be unavailable during early startup or isolated tests.
                return null;
            }
        }

        private static bool IsRecruitDefinition(ToolDefinition def, ToolManifest manifest, ToolScope resolvedScopes)
        {
            if (ContainsToken(manifest.AutonomyDomainId, "recruit"))
                return true;

            bool hasExplicitScope = manifest.Scopes.HasValue && manifest.Scopes.Value != ToolScope.None;
            if (hasExplicitScope && (resolvedScopes & ToolScope.Social) == ToolScope.None)
                return false;

            if ((resolvedScopes & ToolScope.Social) != ToolScope.None)
                return MentionsToken(def, "recruit");

            return false;
        }

        private static bool HasAuthoritativeNonMatchingMetadata(ToolManifest manifest)
        {
            if (manifest.Scopes.HasValue && manifest.Scopes.Value != ToolScope.None)
                return true;

            if (manifest.DefaultDomain is AutonomyDomain domain
                && domain != AutonomyDomain.HighRiskOperations)
            {
                return true;
            }

            return !string.IsNullOrWhiteSpace(manifest.AutonomyDomainId);
        }

        private static bool MentionsToken(ToolDefinition def, string token)
        {
            if (ContainsToken(def.Id, token) || ContainsToken(def.Manifest?.DisplayName, token))
                return true;

            var aliases = def.Manifest?.Aliases;
            if (aliases != null)
            {
                for (int i = 0; i < aliases.Count; i++)
                {
                    if (ContainsToken(aliases[i], token))
                        return true;
                }
            }

            return false;
        }

        private static bool ContainsAnyToken(string? value, params string[] tokens)
        {
            if (string.IsNullOrWhiteSpace(value)) return false;
            for (int i = 0; i < tokens.Length; i++)
            {
                if (ContainsToken(value, tokens[i]))
                    return true;
            }
            return false;
        }

        private static bool ContainsToken(string? value, string token)
        {
            if (string.IsNullOrWhiteSpace(value) || string.IsNullOrWhiteSpace(token))
                return false;

            string[] parts = value!.Split(TokenSeparators, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < parts.Length; i++)
            {
                if (string.Equals(parts[i], token, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        private static bool MatchesPrefix(string value, string prefix)
        {
            if (string.Equals(value, prefix, StringComparison.OrdinalIgnoreCase))
                return true;
            return value.Length > prefix.Length
                && value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
                && (value[prefix.Length] == '.' || value[prefix.Length] == ':' || value[prefix.Length] == '_');
        }
    }
}
