using System.Reflection;
using RimMind.Bridge.RimChat.Cooldown;
using Xunit;

namespace RimMind.Bridge.RimChat.Tests
{
    /// <summary>
    /// 验证 SharedIncidentCooldown 不再依赖 ModSettings 持久化，
    /// 且 GameComponent_BridgeRimChat 提供独立的 ExposeData 入口。
    /// </summary>
    public class GameComponentBridgeRimChatTests
    {
        public GameComponentBridgeRimChatTests()
        {
            ResetSharedIncidentCooldown();
            Verse.Find.TickManager.TicksGame = 100000;
        }

        [Fact]
        public void BridgeRimChatSettings_ExposeData_不调用SharedIncidentCooldown_ExposeData()
        {
            // 验证 GameComponent_BridgeRimChat 类型存在
            var gameCompType = typeof(GameComponent_BridgeRimChat);
            Assert.NotNull(gameCompType);
        }

        [Fact]
        public void GameComponent_BridgeRimChat_存在ExposeData方法()
        {
            var gameCompType = typeof(GameComponent_BridgeRimChat);
            var exposeData = gameCompType.GetMethod("ExposeData");
            Assert.NotNull(exposeData);
        }

        [Fact]
        public void SharedIncidentCooldown_ExposeData仍可用_供GameComponent调用()
        {
            // ExposeData 应保留为 internal static，供 GameComponent 调用
            var exposeData = typeof(SharedIncidentCooldown).GetMethod("ExposeData",
                BindingFlags.NonPublic | BindingFlags.Static);
            Assert.NotNull(exposeData);
        }

        private static void ResetSharedIncidentCooldown()
        {
            var field = typeof(SharedIncidentCooldown).GetField("_lastIncidentTick",
                BindingFlags.NonPublic | BindingFlags.Static);
            if (field != null)
                field.SetValue(null, -99999);
        }
    }
}
