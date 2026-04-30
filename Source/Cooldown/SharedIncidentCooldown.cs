using Verse;

namespace RimMind.Bridge.RimChat.Cooldown
{
    public static class SharedIncidentCooldown
    {
        private static int _lastIncidentTick = -99999;

        public static void RecordIncident()
        {
            try
            {
                _lastIncidentTick = Find.TickManager.TicksGame;
            }
            catch
            {
                _lastIncidentTick = 0;
            }
        }

        public static bool IsOnCooldown(int cooldownTicks)
        {
            try
            {
                return Find.TickManager.TicksGame - _lastIncidentTick < cooldownTicks;
            }
            catch
            {
                return false;
            }
        }

        public static void ExposeData()
        {
            Scribe_Values.Look(ref _lastIncidentTick, "RimMind_BridgeRimChat_LastIncidentTick", -99999);
        }
    }
}
