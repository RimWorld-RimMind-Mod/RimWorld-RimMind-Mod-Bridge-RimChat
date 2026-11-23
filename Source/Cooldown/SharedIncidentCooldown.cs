using Verse;

namespace RimMind.Bridge.RimChat.Cooldown
{
    public static class SharedIncidentCooldown
    {
        private static GameComponent_BridgeRimChat? State =>
            Current.Game?.GetComponent<GameComponent_BridgeRimChat>();

        public static void RecordIncident()
        {
            var tickManager = Find.TickManager;
            if (tickManager != null)
                State?.RecordIncident(tickManager.TicksGame);
        }

        public static bool IsOnCooldown(int cooldownTicks)
        {
            var tickManager = Find.TickManager;
            return tickManager != null && (State?.IsOnCooldown(tickManager.TicksGame, cooldownTicks) ?? false);
        }
    }
}
