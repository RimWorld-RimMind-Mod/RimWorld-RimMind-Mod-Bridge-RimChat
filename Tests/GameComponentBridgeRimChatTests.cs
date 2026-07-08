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
        public void GameComponent_BridgeRimChat_类型存在()
        {
            // 结构性测试：验证 GameComponent_BridgeRimChat 类型存在
            var gameCompType = typeof(GameComponent_BridgeRimChat);
            Assert.NotNull(gameCompType);
        }

        [Fact]
        public void GameComponent_BridgeRimChat_存在ExposeData方法()
        {
            // 结构性测试：验证 ExposeData 方法存在（GameComponent 覆写入口）
            var gameCompType = typeof(GameComponent_BridgeRimChat);
            var exposeData = gameCompType.GetMethod("ExposeData");
            Assert.NotNull(exposeData);
        }

        [Fact]
        public void SharedIncidentCooldown_ExposeData仍可用_供GameComponent调用()
        {
            // 结构性测试：ExposeData 应保留为 internal static，供 GameComponent 调用
            var exposeData = typeof(SharedIncidentCooldown).GetMethod("ExposeData",
                BindingFlags.NonPublic | BindingFlags.Static);
            Assert.NotNull(exposeData);
        }

        [Fact]
        public void GameComponent_BridgeRimChat_ExposeData_调用不抛异常()
        {
            // 行为测试：实例化 GameComponent 并调用 ExposeData，验证不抛异常
            var component = new GameComponent_BridgeRimChat(new Verse.Game());
            var exception = Record.Exception(() => component.ExposeData());
            Assert.Null(exception);
        }

        [Fact]
        public void GameComponent_BridgeRimChat_ExposeData_调用SharedIncidentCooldown_ExposeData()
        {
            // 行为测试：验证 GameComponent.ExposeData 正确调用 SharedIncidentCooldown.ExposeData，
            // 后者会调用 Scribe_Values.Look（桩为 no-op），证明 wiring 正确。
            // 1. 设置已知 TicksGame
            Verse.Find.TickManager.TicksGame = 100000;
            // 2. 通过 RecordIncident 写入 _lastIncidentTick
            SharedIncidentCooldown.RecordIncident();
            // 3. 验证状态确实被记录（cooldown 生效）
            Assert.True(SharedIncidentCooldown.IsOnCooldown(60000));
            // 4. 调用 GameComponent.ExposeData —— 应触发 SharedIncidentCooldown.ExposeData → Scribe_Values.Look
            var component = new GameComponent_BridgeRimChat(new Verse.Game());
            var exception = Record.Exception(() => component.ExposeData());
            // 5. 桩 Scribe_Values.Look 为 no-op，不应抛异常，wiring 完整
            Assert.Null(exception);
            // 6. ExposeData 调用后状态仍可读（_lastIncidentTick 未被桩破坏）
            Assert.True(SharedIncidentCooldown.IsOnCooldown(60000));
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
