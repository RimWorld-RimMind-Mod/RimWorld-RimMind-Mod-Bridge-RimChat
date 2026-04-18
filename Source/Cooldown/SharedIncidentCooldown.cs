using Verse;

namespace RimMind.Bridge.RimChat.Cooldown
{
    public static class SharedIncidentCooldown
    {
        private static int _lastIncidentTick = -99999;

        public static void RecordIncident()
        {
            _lastIncidentTick = Find.TickManager.TicksGame;
        }

        public static bool IsOnCooldown(int cooldownTicks)
        {
            return Find.TickManager.TicksGame - _lastIncidentTick < cooldownTicks;
        }

        public static int LastIncidentTick => _lastIncidentTick;

        public static void Reset()
        {
            _lastIncidentTick = -99999;
        }
    }
}
