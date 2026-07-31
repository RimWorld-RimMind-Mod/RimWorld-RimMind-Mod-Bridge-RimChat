using Verse;

namespace RimMind.Bridge.RimChat.Cooldown
{
    /// <summary>
    /// per-game 状态承载：SharedIncidentCooldown 的持久化入口。
    /// RimWorld 反射自动发现 (Game game) 构造函数。
    /// </summary>
    public class GameComponent_BridgeRimChat : GameComponent
    {
        public GameComponent_BridgeRimChat(Game game) : base() { }

        public override void ExposeData()
        {
            base.ExposeData();
            SharedIncidentCooldown.ExposeData();
        }
    }
}
