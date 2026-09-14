using Verse;

namespace RimMind.Bridge.RimChat.Cooldown
{
    public static class SharedIncidentCooldown
    {
        private static GameComponent_BridgeRimChat? State =>
            Current.Game?.GetComponent<GameComponent_BridgeRimChat>();

        public static void RecordIncident()
            => State?.RecordIncident(Find.TickManager.TicksGame);

        public static bool IsOnCooldown(int cooldownTicks)
            => State?.IsOnCooldown(Find.TickManager.TicksGame, cooldownTicks) ?? false;
    }
}
