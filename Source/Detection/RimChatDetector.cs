using System;
using Verse;

namespace RimMind.Bridge.RimChat.Detection
{
    public static class RimChatDetector
    {
        public const string RimChatPackageId = "yancy.rimchat";

        private static bool? _cachedResult;
        private static int _cacheTick = -1;
        private const int CacheIntervalTicks = 6000;

        private static int SafeTicksGame
        {
            get
            {
                try { return Find.TickManager?.TicksGame ?? 0; }
                catch { return 0; }
            }
        }

        public static bool IsRimChatActive
        {
            get
            {
                int now = SafeTicksGame;
                if (_cachedResult == null || now - _cacheTick > CacheIntervalTicks)
                {
                    _cachedResult = ModsConfig.IsActive(RimChatPackageId);
                    _cacheTick = now;
                }
                return _cachedResult.Value;
            }
        }

    }
}
