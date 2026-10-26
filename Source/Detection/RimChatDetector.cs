using System;
using Verse;

namespace RimMind.Bridge.RimChat.Detection
{
    public static class RimChatDetector
    {
        public const string RimChatPackageId = "yancy.rimchat";

        private const int CacheIntervalTicks = 6000;
        private static Func<string, bool> _isActive = ModsConfig.IsActive;
        private static bool? _cachedResult;
        private static int _cacheTick = -CacheIntervalTicks;

        public static bool IsRimChatActive
        {
            get
            {
                int now = SafeTicksGame;
                if (_cachedResult == null || now < _cacheTick || now - _cacheTick > CacheIntervalTicks)
                {
                    _cachedResult = _isActive(RimChatPackageId);
                    _cacheTick = now;
                }
                return _cachedResult.Value;
            }
        }

        public static void InvalidateCache()
        {
            _cachedResult = null;
            _cacheTick = -CacheIntervalTicks;
        }

        internal static void UseActiveProbeForTesting(Func<string, bool> probe)
        {
            _isActive = probe ?? throw new ArgumentNullException(nameof(probe));
            InvalidateCache();
        }

        internal static void ResetActiveProbeForTesting()
        {
            _isActive = ModsConfig.IsActive;
            InvalidateCache();
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
