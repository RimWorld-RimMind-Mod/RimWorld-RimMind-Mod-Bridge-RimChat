using System;
using Verse;

namespace RimMind.Bridge.RimChat.Detection
{
    public static class RimChatDetector
    {
        public const string RimChatPackageId = "yancy.rimchat";

        private static Func<string, bool> _isActive = ModsConfig.IsActive;

        public static bool IsRimChatActive => _isActive(RimChatPackageId);

        internal static void UseActiveProbeForTesting(Func<string, bool> probe)
        {
            _isActive = probe ?? throw new ArgumentNullException(nameof(probe));
        }

        internal static void ResetActiveProbeForTesting() => _isActive = ModsConfig.IsActive;
    }
}
