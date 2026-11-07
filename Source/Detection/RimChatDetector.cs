using System;
using HarmonyLib;
using RimMind.Application.Features.Bridges;
using Verse;

namespace RimMind.Bridge.RimChat.Detection
{
    public static class RimChatDetector
    {
        public const string RimChatPackageId = "yancy.rimchat";
        private const string RimChatApiTypeName = "RimChat.API.RimChatAPI";
        private const int CacheIntervalTicks = 6000;

        internal static readonly SoftDependencyDetector Detector = new SoftDependencyDetector(
            RimChatPackageId,
            new[] { RimChatApiTypeName },
            ModsConfig.IsActive,
            AccessTools.TypeByName,
            () => SafeTicksGame,
            CacheIntervalTicks);

        public static bool IsRimChatActive => Detector.IsModActive();

        public static void InvalidateCache()
        {
            Detector.ResetCache();
        }

        internal static void UseActiveProbeForTesting(Func<string, bool> probe)
        {
            if (probe == null) throw new ArgumentNullException(nameof(probe));
            Detector.ConfigureForTesting(isModActive: probe);
        }

        internal static void ResetActiveProbeForTesting()
        {
            Detector.ResetDelegatesForTesting();
        }

        private static int SafeTicksGame
        {
            get
            {
                try { return Find.TickManager?.TicksGame ?? 0; }
                catch { return 0; }
            }
        }
    }
}
