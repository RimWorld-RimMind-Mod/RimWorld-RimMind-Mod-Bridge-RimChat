using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RimMind.Bridge.RimChat.Detection;
using RimMind.Bridge.RimChat.Settings;
using RimMind.Application.Common.Interfaces.Context;
using RimMind.Domain.ValueObjects;
using RimMind.Presentation.Api;
using Verse;

namespace RimMind.Bridge.RimChat.Bridge
{
    public static class ContextPullBridge
    {
        private const string ModId = "RimMindBridgeRimChat";
        private static bool _diplomacyDisabled;
        private static bool _rpgDisabled;

        public static void Register()
        {
            _diplomacyDisabled = false;
            _rpgDisabled = false;

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
            RimMindAPI.Context.ContextKeys.Register(new ContextProviderDef(
                "rimchat_diplomacy", ContextLayer.L2_Environment, 0.4f,
                async (ctx, ct) =>
                {
                    if (!RimChatDetector.IsRimChatActive || _diplomacyDisabled) return null;
                    return await RimMindAPI.MainThreadSnapshot.GetAsync<string?>(
                        () => BuildDiplomacyContext(),
                        null,
                        ct: ct).ConfigureAwait(false);
                }, ModId, stalenessTicks: 1500, invalidationTriggers: new[] { "RimChatEvent" }));
        }

        private static string? BuildDiplomacyContext()
        {
            if (!RimChatDetector.IsRimChatActive || _diplomacyDisabled) return null;

            try
            {
                var manager = RimChatApiShim.TryGetManagerInstance(RimChatApiShim.DiplomacyManagerType);
                if (manager == null) return null;

                var sessions = RimChatApiShim.GetInstanceFieldValue(manager, "dialogueSessions") as IList;
                if (sessions == null || sessions.Count == 0) return null;

                var sb = new StringBuilder();
                sb.AppendLine("[RimChat Diplomacy]");
                int sessionCount = 0;
                int startSessionIdx = System.Math.Max(0, sessions.Count - 3);
                for (int si = startSessionIdx; si < sessions.Count; si++)
                {
                    if (sessionCount >= 3) break;
                    var session = sessions[si];
                    if (session == null) continue;

                    var sessionType = session.GetType();
                    var factionField = RimChatApiShim.GetCachedField(sessionType, "faction",
                        BindingFlags.Public | BindingFlags.Instance);
                    var messagesField = RimChatApiShim.GetCachedField(sessionType, "messages",
                        BindingFlags.Public | BindingFlags.Instance);

                    if (factionField == null || messagesField == null)
                    {
                        _diplomacyDisabled = true;
                        return null;
                    }

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
                        if (msg == null) continue;
                        var msgType = msg.GetType();
                        var senderField = RimChatApiShim.GetCachedField(msgType, "sender",
                            BindingFlags.Public | BindingFlags.Instance);
                        var messageField = RimChatApiShim.GetCachedField(msgType, "message",
                            BindingFlags.Public | BindingFlags.Instance);
                        var isPlayerField = RimChatApiShim.GetCachedField(msgType, "isPlayer",
                            BindingFlags.Public | BindingFlags.Instance);

                        if (senderField == null || messageField == null)
                        {
                            _diplomacyDisabled = true;
                            return null;
                        }

                        string sender = senderField.GetValue(msg)?.ToString() ?? "?";
                        string content = messageField.GetValue(msg)?.ToString() ?? "";
                        bool isPlayer = isPlayerField?.GetValue(msg) is bool playerMessage && playerMessage;

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
                RimMindErrors.Warn($"[RimMind-Bridge-RimChat] Diplomacy context pull failed: {ex.Message}");
                return null;
            }
        }

        private static void RegisterRpgProvider()
        {
            RimMindAPI.Context.ContextKeys.Register(new ContextProviderDef(
                "rimchat_rpg_history", ContextLayer.L4_History, 0.5f,
                async (ctx, ct) =>
                {
                    if (ctx.PawnId <= 0 || !RimChatDetector.IsRimChatActive || _rpgDisabled) return null;
                    return await RimMindAPI.MainThreadSnapshot.GetAsync<string?>(() =>
                    {
                        var pawn = TryFindPawnById(ctx.PawnId);
                        if (pawn == null) return null;
                        return BuildRpgContext(pawn);
                    }, null, ct: ct).ConfigureAwait(false);
                }, ModId, stalenessTicks: 3000, invalidationTriggers: new[] { "RimChatEvent" }));
        }

        internal static Pawn? TryFindPawnById(int pawnId)
        {
            var pawn = RimMindPawnLookup.FindPawnByNumber(pawnId);
            return RimMindPawnLookup.IsActiveSpawnedColonist(pawn) ? pawn : null;
        }

        private static string? BuildRpgContext(Pawn pawn)
        {
            if (!RimChatDetector.IsRimChatActive || _rpgDisabled) return null;

            try
            {
                var manager = RimChatApiShim.TryGetManagerInstance(RimChatApiShim.RpgArchiveManagerType);
                if (manager == null) return null;

                var cache = RimChatApiShim.GetInstanceFieldValue(manager, "_archiveCache",
                    BindingFlags.NonPublic | BindingFlags.Instance) as IDictionary;
                if (cache == null) return null;

                object? archive = null;
                foreach (DictionaryEntry entry in cache)
                {
                    var archiveCandidate = entry.Value;
                    if (archiveCandidate == null) continue;

                    var pawnLoadIdField = RimChatApiShim.GetCachedField(archiveCandidate.GetType(), "PawnLoadId",
                        BindingFlags.Public | BindingFlags.Instance);
                    if (pawnLoadIdField == null)
                    {
                        _rpgDisabled = true;
                        return null;
                    }

                    var archivePawnIdValue = pawnLoadIdField.GetValue(archiveCandidate);
                    if (archivePawnIdValue == null) continue;

                    int archivePawnId = System.Convert.ToInt32(archivePawnIdValue);
                    if (archivePawnId == pawn.thingIDNumber)
                    {
                        archive = archiveCandidate;
                        break;
                    }
                }

                if (archive == null) return null;

                var sessionsField = RimChatApiShim.GetCachedField(archive.GetType(), "Sessions",
                    BindingFlags.Public | BindingFlags.Instance);
                if (sessionsField == null)
                {
                    _rpgDisabled = true;
                    return null;
                }

                var sessions = sessionsField.GetValue(archive) as IList;
                if (sessions == null || sessions.Count == 0) return null;

                var sb = new StringBuilder();
                sb.AppendLine("[RimChat RPG History]");
                int sessionCount = 0;
                for (int si = sessions.Count - 1; si >= 0; si--)
                {
                    if (sessionCount >= 2) break;

                    var session = sessions[si];
                    if (session == null) continue;
                    var turnsField = RimChatApiShim.GetCachedField(session.GetType(), "Turns",
                        BindingFlags.Public | BindingFlags.Instance);
                    if (turnsField == null)
                    {
                        _rpgDisabled = true;
                        return null;
                    }

                    var turns = turnsField.GetValue(session) as IList;
                    if (turns == null || turns.Count == 0) continue;

                    int turnCount = 0;
                    int startIdx = System.Math.Max(0, turns.Count - 4);
                    for (int ti = startIdx; ti < turns.Count; ti++)
                    {
                        if (turnCount >= 4) break;
                        var turn = turns[ti];
                        if (turn == null) continue;

                        var turnType = turn.GetType();
                        var isPlayerField = RimChatApiShim.GetCachedField(turnType, "IsPlayer",
                            BindingFlags.Public | BindingFlags.Instance);
                        var speakerField = RimChatApiShim.GetCachedField(turnType, "SpeakerName",
                            BindingFlags.Public | BindingFlags.Instance);
                        var textField = RimChatApiShim.GetCachedField(turnType, "Text",
                            BindingFlags.Public | BindingFlags.Instance);

                        if (textField == null)
                        {
                            _rpgDisabled = true;
                            return null;
                        }

                        bool isPlayer = isPlayerField?.GetValue(turn) is bool playerTurn && playerTurn;
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
                RimMindErrors.Warn($"[RimMind-Bridge-RimChat] RPG context pull failed: {ex.Message}");
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
            _diplomacyDisabled = false;
            _rpgDisabled = false;
            RimMindAPI.Context.ContextKeys.Unregister("rimchat_diplomacy");
            RimMindAPI.Context.ContextKeys.Unregister("rimchat_rpg_history");
        }

        public static void Refresh()
        {
            Unregister();
            Register();
        }
    }
}
