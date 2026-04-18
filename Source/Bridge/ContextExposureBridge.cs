using System.Text;
using RimMind.Bridge.RimChat.Detection;
using RimMind.Bridge.RimChat.Settings;
using RimMind.Core;
using RimMind.Core.Prompt;
using RimMind.Advisor.Data;
using RimMind.Memory.Data;
using RimMind.Personality.Data;
using Verse;

namespace RimMind.Bridge.RimChat.Bridge
{
    public static class ContextExposureBridge
    {
        public static void Register()
        {
            if (!RimChatDetector.IsRimChatActive) return;

            var settings = BridgeRimChatSettings.Get();
            if (!settings.enableContextExposure) return;

            if (settings.exposePersonality)
                RegisterPersonalityProvider();

            if (settings.exposeMemory)
                RegisterMemoryProvider();

            if (settings.exposeStoryteller)
                RegisterStorytellerProvider();

            if (settings.exposeAdvisorLog)
                RegisterAdvisorLogProvider();
        }

        private static void RegisterPersonalityProvider()
        {
            RimMindAPI.RegisterPawnContextProvider("rimmind_personality", pawn =>
            {
                var profile = AIPersonalityWorldComponent.Instance?.GetOrCreate(pawn);
                if (profile == null || profile.IsEmpty) return null;

                var sb = new StringBuilder("[RimMind Personality]");
                if (!string.IsNullOrEmpty(profile.description))
                    sb.AppendLine(profile.description);
                if (!string.IsNullOrEmpty(profile.workTendencies))
                    sb.AppendLine($"[Work] {profile.workTendencies}");
                if (!string.IsNullOrEmpty(profile.socialTendencies))
                    sb.AppendLine($"[Social] {profile.socialTendencies}");
                if (!string.IsNullOrEmpty(profile.aiNarrative))
                    sb.AppendLine($"[AI] {profile.aiNarrative}");
                return sb.ToString().TrimEnd();
            }, PromptSection.PriorityMemory, "RimMind.Bridge.RimChat");
        }

        private static void RegisterMemoryProvider()
        {
            RimMindAPI.RegisterPawnContextProvider("rimmind_memory", pawn =>
            {
                var store = RimMindMemoryWorldComponent.Instance?.GetOrCreatePawnStore(pawn);
                if (store == null || store.IsEmpty) return null;

                var sb = new StringBuilder("[RimMind Memory]");
                int count = 0;
                foreach (var m in store.active)
                {
                    if (count >= 5) break;
                    sb.AppendLine($"- {m.content}");
                    count++;
                }
                if (store.dark.Count > 0)
                {
                    sb.AppendLine("[Long-term]");
                    foreach (var m in store.dark)
                        sb.AppendLine($"- {m.content}");
                }
                return sb.ToString().TrimEnd();
            }, PromptSection.PriorityMemory, "RimMind.Bridge.RimChat");
        }

        private static void RegisterStorytellerProvider()
        {
            RimMindAPI.RegisterPawnContextProvider("rimmind_storyteller", pawn =>
            {
                var store = RimMindMemoryWorldComponent.Instance?.NarratorStore;
                if (store == null || store.IsEmpty) return null;

                var sb = new StringBuilder("[RimMind Storyteller]");
                int count = 0;
                foreach (var m in store.active)
                {
                    if (count >= 5) break;
                    sb.AppendLine($"- {m.content}");
                    count++;
                }
                return sb.ToString().TrimEnd();
            }, PromptSection.PriorityAuxiliary, "RimMind.Bridge.RimChat");
        }

        private static void RegisterAdvisorLogProvider()
        {
            RimMindAPI.RegisterPawnContextProvider("rimmind_advisor_log", pawn =>
            {
                var history = AdvisorHistoryStore.Instance?.GetRecords(pawn);
                if (history == null || history.Count == 0) return null;

                var sb = new StringBuilder("[RimMind Advisor]");
                int count = 0;
                foreach (var r in history)
                {
                    if (count >= 5) break;
                    sb.AppendLine($"- {r.action}: {r.reason} ({r.result})");
                    count++;
                }
                return sb.ToString().TrimEnd();
            }, PromptSection.PriorityAuxiliary, "RimMind.Bridge.RimChat");
        }

        public static void Unregister()
        {
            RimMindAPI.UnregisterModProviders("RimMind.Bridge.RimChat");
        }
    }
}
