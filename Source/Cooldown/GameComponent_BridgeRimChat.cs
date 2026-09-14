using Verse;

namespace RimMind.Bridge.RimChat.Cooldown
{
    /// <summary>
    /// Owns incident cooldown state for one game, including save/load.
    /// RimWorld 反射自动发现 (Game game) 构造函数。
    /// </summary>
    public class GameComponent_BridgeRimChat : GameComponent
    {
        private int _lastIncidentTick = -99999;

        public GameComponent_BridgeRimChat(Game game) : base() { }

        internal void RecordIncident(int tick) => _lastIncidentTick = tick;

        internal bool IsOnCooldown(int tick, int cooldownTicks)
            => _lastIncidentTick >= 0 && tick >= _lastIncidentTick
                && tick - _lastIncidentTick < cooldownTicks;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref _lastIncidentTick, "RimMind_BridgeRimChat_LastIncidentTick", -99999);
        }
    }
}
