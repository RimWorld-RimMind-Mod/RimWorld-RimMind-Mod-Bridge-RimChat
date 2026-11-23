using System;
using RimMind.Application.Features.Bridges;
using RimMind.Presentation.Api;

namespace RimMind.Bridge.RimChat.Detection
{
    public static class RimChatDetector
    {
        public const string RimChatPackageId = "yancy.rimchat";
        private const string RimChatDiplomacyManagerTypeName = "RimChat.DiplomacySystem.GameComponent_DiplomacyManager";
        private const string RimChatRpgArchiveManagerTypeName = "RimChat.Memory.RpgNpcDialogueArchiveManager";
        private const int CacheIntervalTicks = 6000;

        internal static readonly SoftDependencyDetector Detector = RimMindAPI.Bridges.CreateDetector(
            RimChatPackageId,
            new[] { RimChatDiplomacyManagerTypeName, RimChatRpgArchiveManagerTypeName },
            CacheIntervalTicks);

        public static bool IsRimChatActive => Detector.IsModActive();

        public static bool IsRimChatApiAvailable => Detector.IsAvailable();

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
    }
}
