using System.Collections.Generic;
using RimMind.Application.Common.Interfaces.Extension;
using RimMind.Application.Features.Bridges;
using RimMind.Bridge.RimChat.Detection;
using RimMind.Presentation.Api;
using Verse;

namespace RimMind.Bridge.RimChat.Bridge
{
    /// <summary>
    /// Coordinates registration of RimChat bridge skip gates and modules via Core's
    /// <see cref="RimMindAPI.Bridges.CreateCoordinator"/>, matching Bridge-RimTalk's lifecycle symmetrically.
    /// </summary>
    public static class RimChatBridgeCoordinator
    {
        private static ContextPullBridge? _contextPull;
        private static List<IBridgeModule>? _modules;
        private static List<ISkipCheck>? _gates;
        private static ExternalModBridgeCoordinator? _coordinator;

        public static IReadOnlyList<IBridgeModule> Modules => GetModules();
        public static IReadOnlyList<ISkipCheck> Gates => GetGates();

        internal static ContextPullBridge ContextPull => _contextPull ??= new ContextPullBridge();

        private static List<IBridgeModule> GetModules() =>
            _modules ??= new List<IBridgeModule>
            {
                new IncidentListenerBridgeModule(),
                ContextPull
            };

        private static List<ISkipCheck> GetGates() =>
            _gates ??= new List<ISkipCheck>
            {
                new RimChatDialogueSkipCheck(),
                new RimChatFloatMenuSkipCheck(),
                new RimChatActionSkipCheck(),
                new RimChatStorytellerIncidentSkipCheck()
            };

        private static ExternalModBridgeCoordinator GetCoordinator() =>
            _coordinator ??= RimMindAPI.Bridges.CreateCoordinator(
                () => RimChatDetector.IsRimChatActive,
                GetModules(),
                gates: GetGates(),
                logInfo: message => Log.Message($"[RimMind-Bridge-RimChat] {message}"),
                logWarn: message => Log.Warning($"[RimMind-Bridge-RimChat] {message}"),
                inactiveMessage: "RimChat not active, bridge modules skipped.");

        public static void Register()
        {
            GetCoordinator().RegisterAll();
            Log.Message("[RimMind-Bridge-RimChat] Initialized.");
        }

        public static void Unregister() => GetCoordinator().UnregisterAll();

        public static void Refresh() => GetCoordinator().Refresh();

        /// <summary>
        /// Refreshes only the ContextPull providers when settings change,
        /// without re-registering live SkipCheck delegates.
        /// </summary>
        public static void RefreshContextPull() => ContextPull.Refresh();

        internal static void ResetForTesting()
        {
            _coordinator?.UnregisterAll();
            _coordinator = null;
            _modules = null;
            _gates = null;
            _contextPull = null;
        }

        private sealed class IncidentListenerBridgeModule : IBridgeModule
        {
            private readonly RimChatIncidentExecutedListener _listener = new RimChatIncidentExecutedListener();

            public string Id => "incident_listener";
            public string OwnerModId => "RimMindBridgeRimChat";
            public bool IsRegistered { get; private set; }

            public void Register()
            {
                if (IsRegistered) return;
                if (!RimChatDetector.IsRimChatActive) return;

                RimMindAPI.Ext.Get<IIncidentExecutedListener>().Register(_listener);
                IsRegistered = true;
            }

            public void Unregister()
            {
                if (!IsRegistered) return;
                RimMindAPI.Ext.Get<IIncidentExecutedListener>().Unregister(_listener.Id);
                IsRegistered = false;
            }
        }
    }
}
