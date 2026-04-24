using System.Collections;
using System.Reflection;
using System.Text;
using HarmonyLib;
using RimMind.Bridge.RimChat.Detection;
using RimMind.Bridge.RimChat.Settings;
using RimMind.Core;
using RimMind.Core.Prompt;
using Verse;

namespace RimMind.Bridge.RimChat.Bridge
{
    public static class ContextPullBridge
    {
        private const string ModId = "RimMind.BridgeRimChat";

        public static void Register()
        {
            if (!RimChatDetector.IsRimChatActive) return;

            var settings = BridgeRimChatSettings.Get();
            if (!settings.enableContextPull) return;

            if (settings.pullDiplomacyHistory)
                RegisterDiplomacyProvider();

            if (settings.pullRpgHistory)
                RegisterRpgProvider();
        }

        private static void RegisterDiplomacyProvider()
        {
            RimMindAPI.RegisterStaticProvider("rimchat_diplomacy", () =>
            {
                return (string?)BuildDiplomacyContext();
            }, PromptSection.PriorityAuxiliary, ModId);
        }

        private static string? BuildDiplomacyContext()
        {
            if (!RimChatDetector.IsRimChatActive) return null;

            try
            {
                var managerType = AccessTools.TypeByName("RimChat.DiplomacySystem.GameComponent_DiplomacyManager");
                if (managerType == null) return null;

                var instanceProp = managerType.GetProperty("Instance",
                    BindingFlags.Public | BindingFlags.Static);
                if (instanceProp == null) return null;

                var manager = instanceProp.GetValue(null);
                if (manager == null) return null;

                var sessionsField = managerType.GetField("dialogueSessions",
                    BindingFlags.Public | BindingFlags.Instance);
                if (sessionsField == null) return null;

                var sessions = sessionsField.GetValue(manager) as IList;
                if (sessions == null || sessions.Count == 0) return null;

                var sb = new StringBuilder("[RimChat Diplomacy]");
                int sessionCount = 0;
                foreach (var session in sessions)
                {
                    if (sessionCount >= 3) break;

                    var factionField = session.GetType().GetField("faction",
                        BindingFlags.Public | BindingFlags.Instance);
                    var messagesField = session.GetType().GetField("messages",
                        BindingFlags.Public | BindingFlags.Instance);

                    if (factionField == null || messagesField == null) continue;

                    var faction = factionField.GetValue(session);
                    var messages = messagesField.GetValue(session) as IList;
                    if (faction == null || messages == null) continue;

                    string factionName = faction.ToString() ?? "?";
                    sb.AppendLine($"## {factionName}");

                    int msgCount = 0;
                    int startIdx = System.Math.Max(0, messages.Count - 4);
                    for (int i = startIdx; i < messages.Count; i++)
                    {
                        if (msgCount >= 4) break;
                        var msg = messages[i];
                        var senderField = msg.GetType().GetField("sender",
                            BindingFlags.Public | BindingFlags.Instance);
                        var messageField = msg.GetType().GetField("message",
                            BindingFlags.Public | BindingFlags.Instance);
                        var isPlayerField = msg.GetType().GetField("isPlayer",
                            BindingFlags.Public | BindingFlags.Instance);

                        if (senderField == null || messageField == null) continue;

                        string sender = senderField.GetValue(msg)?.ToString() ?? "?";
                        string content = messageField.GetValue(msg)?.ToString() ?? "";
                        bool isPlayer = isPlayerField != null && (bool)isPlayerField.GetValue(msg);

                        if (string.IsNullOrEmpty(content)) continue;
                        string label = isPlayer ? "Player" : sender;
                        sb.AppendLine($"- [{label}] {Truncate(content, 120)}");
                        msgCount++;
                    }
                    sessionCount++;
                }
                return sessionCount > 0 ? sb.ToString().TrimEnd() : null;
            }
            catch (System.Exception ex)
            {
                Log.WarningOnce($"[RimMind-Bridge-RimChat] Diplomacy context pull failed: {ex.Message}", 87421);
                return null;
            }
        }

        private static void RegisterRpgProvider()
        {
            RimMindAPI.RegisterPawnContextProvider("rimchat_rpg_history", pawn =>
            {
                return BuildRpgContext(pawn);
            }, PromptSection.PriorityMemory, ModId);
        }

        private static string? BuildRpgContext(Pawn pawn)
        {
            if (!RimChatDetector.IsRimChatActive) return null;

            try
            {
                var managerType = AccessTools.TypeByName("RimChat.Memory.RpgNpcDialogueArchiveManager");
                if (managerType == null) return null;

                var instanceProp = managerType.GetProperty("Instance",
                    BindingFlags.Public | BindingFlags.Static);
                if (instanceProp == null) return null;

                var manager = instanceProp.GetValue(null);
                if (manager == null) return null;

                var cacheField = managerType.GetField("_archiveCache",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                if (cacheField == null) return null;

                var cache = cacheField.GetValue(manager) as IDictionary;
                if (cache == null) return null;

                object? archive = null;
                foreach (DictionaryEntry entry in cache)
                {
                    var pawnLoadIdField = entry.Value.GetType().GetField("PawnLoadId",
                        BindingFlags.Public | BindingFlags.Instance);
                    if (pawnLoadIdField != null)
                    {
                        int archivePawnId = (int)pawnLoadIdField.GetValue(entry.Value);
                        if (archivePawnId == pawn.thingIDNumber)
                        {
                            archive = entry.Value;
                            break;
                        }
                    }
                }

                if (archive == null) return null;

                var sessionsField = archive.GetType().GetField("Sessions",
                    BindingFlags.Public | BindingFlags.Instance);
                if (sessionsField == null) return null;

                var sessions = sessionsField.GetValue(archive) as IList;
                if (sessions == null || sessions.Count == 0) return null;

                var sb = new StringBuilder("[RimChat RPG History]");
                int sessionCount = 0;
                for (int si = sessions.Count - 1; si >= 0; si--)
                {
                    if (sessionCount >= 2) break;

                    var session = sessions[si];
                    var turnsField = session.GetType().GetField("Turns",
                        BindingFlags.Public | BindingFlags.Instance);
                    if (turnsField == null) continue;

                    var turns = turnsField.GetValue(session) as IList;
                    if (turns == null || turns.Count == 0) continue;

                    int turnCount = 0;
                    int startIdx = System.Math.Max(0, turns.Count - 4);
                    for (int ti = startIdx; ti < turns.Count; ti++)
                    {
                        if (turnCount >= 4) break;
                        var turn = turns[ti];

                        var isPlayerField = turn.GetType().GetField("IsPlayer",
                            BindingFlags.Public | BindingFlags.Instance);
                        var speakerField = turn.GetType().GetField("SpeakerName",
                            BindingFlags.Public | BindingFlags.Instance);
                        var textField = turn.GetType().GetField("Text",
                            BindingFlags.Public | BindingFlags.Instance);

                        if (textField == null) continue;

                        bool isPlayer = isPlayerField != null && (bool)isPlayerField.GetValue(turn);
                        string speaker = speakerField?.GetValue(turn)?.ToString() ?? "?";
                        string text = textField.GetValue(turn)?.ToString() ?? "";

                        if (string.IsNullOrEmpty(text)) continue;
                        string label = isPlayer ? "Player" : speaker;
                        sb.AppendLine($"- [{label}] {Truncate(text, 120)}");
                        turnCount++;
                    }
                    sessionCount++;
                }
                return sessionCount > 0 ? sb.ToString().TrimEnd() : null;
            }
            catch (System.Exception ex)
            {
                Log.WarningOnce($"[RimMind-Bridge-RimChat] RPG context pull failed: {ex.Message}", 87422);
                return null;
            }
        }

        private static string Truncate(string s, int maxLen)
        {
            if (string.IsNullOrEmpty(s)) return s;
            return s.Length <= maxLen ? s : s.Substring(0, maxLen) + "...";
        }

        public static void Unregister()
        {
            RimMindAPI.UnregisterModProviders(ModId);
        }

        public static void Refresh()
        {
            Unregister();
            Register();
        }
    }
}
