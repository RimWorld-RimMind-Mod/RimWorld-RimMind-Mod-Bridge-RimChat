using RimMind.Application.Common.Interfaces.Extension;
using RimMind.Bridge.RimChat.Bridge;
using RimMind.Bridge.RimChat.Cooldown;
using RimMind.Bridge.RimChat.Detection;
using RimMind.Bridge.RimChat.Settings;
using Xunit;

namespace RimMind.Bridge.RimChat.Tests
{
    /// <summary>
    /// 5 个 ISkipCheck Extension + IIncidentExecutedListener 的 Id/Owner/Kind/ShouldSkip 测试。
    /// 验证 Extension 的元数据正确性与 ShouldSkip 到 Gate 的委托转发。
    /// </summary>
    public class SkipCheckExtensionTests
    {
        public SkipCheckExtensionTests()
        {
            RimChatDetector.IsRimChatActive = false;
            BridgeRimChatSettings.Reset();
            Verse.Find.TickManager.TicksGame = 100000;
        }

        // ── RimChatActionSkipCheck ──

        [Fact]
        public void RimChatActionSkipCheck_Id和Kind正确()
        {
            var ext = new RimChatActionSkipCheck();
            Assert.Equal("rimchat_bridge_action", ext.Id);
            Assert.Equal("RimMindBridgeRimChat", ext.OwnerModId);
            Assert.Equal(SkipCheckKind.Action, ext.Kind);
        }

        [Fact]
        public void RimChatActionSkipCheck_RimChat不活跃_不跳过()
        {
            RimChatDetector.IsRimChatActive = false;
            var ext = new RimChatActionSkipCheck();
            var args = new SkipCheckArgs { IntentId = "adjust_faction" };
            Assert.False(ext.ShouldSkip(in args));
        }

        [Fact]
        public void RimChatActionSkipCheck_外交动作_跳过()
        {
            RimChatDetector.IsRimChatActive = true;
            var s = BridgeRimChatSettings.Get();
            s.enableActionGate = true;
            s.skipDiplomacyActions = true;
            var ext = new RimChatActionSkipCheck();
            var args = new SkipCheckArgs { IntentId = "adjust_faction" };
            Assert.True(ext.ShouldSkip(in args));
        }

        [Fact]
        public void RimChatActionSkipCheck_IntentId为空_不跳过()
        {
            RimChatDetector.IsRimChatActive = true;
            var s = BridgeRimChatSettings.Get();
            s.enableActionGate = true;
            s.skipDiplomacyActions = true;
            var ext = new RimChatActionSkipCheck();
            var args = new SkipCheckArgs { IntentId = null };

            Assert.False(ext.ShouldSkip(in args));
        }

        // ── RimChatDialogueSkipCheck ──

        [Fact]
        public void RimChatDialogueSkipCheck_Id和Kind正确()
        {
            var ext = new RimChatDialogueSkipCheck();
            Assert.Equal("rimchat_bridge_dialogue", ext.Id);
            Assert.Equal("RimMindBridgeRimChat", ext.OwnerModId);
            Assert.Equal(SkipCheckKind.Dialogue, ext.Kind);
        }

        [Fact]
        public void RimChatDialogueSkipCheck_Chibat开启_跳过()
        {
            RimChatDetector.IsRimChatActive = true;
            var s = BridgeRimChatSettings.Get();
            s.enableChitchatGate = true;
            var ext = new RimChatDialogueSkipCheck();
            var args = new SkipCheckArgs { Trigger = "Chitchat" };
            Assert.True(ext.ShouldSkip(in args));
        }

        // ── RimChatFloatMenuSkipCheck ──

        [Fact]
        public void RimChatFloatMenuSkipCheck_Id和Kind正确()
        {
            var ext = new RimChatFloatMenuSkipCheck();
            Assert.Equal("rimchat_bridge_floatmenu", ext.Id);
            Assert.Equal("RimMindBridgeRimChat", ext.OwnerModId);
            Assert.Equal(SkipCheckKind.FloatMenu, ext.Kind);
        }

        [Fact]
        public void RimChatFloatMenuSkipCheck_条件满足_跳过()
        {
            RimChatDetector.IsRimChatActive = true;
            var s = BridgeRimChatSettings.Get();
            s.enablePlayerInputGate = true;
            s.skipPlayerDialogue = true;
            s.forceRimMindPlayerDialogue = false;
            var ext = new RimChatFloatMenuSkipCheck();
            Assert.True(ext.ShouldSkip(default(SkipCheckArgs)));
        }

        // ── RimChatStorytellerIncidentSkipCheck ──

        [Fact]
        public void RimChatStorytellerIncidentSkipCheck_Id和Kind正确()
        {
            var ext = new RimChatStorytellerIncidentSkipCheck();
            Assert.Equal("rimchat_bridge_storyteller", ext.Id);
            Assert.Equal("RimMindBridgeRimChat", ext.OwnerModId);
            Assert.Equal(SkipCheckKind.StorytellerIncident, ext.Kind);
        }

        [Fact]
        public void RimChatStorytellerIncidentSkipCheck_冷却中_跳过()
        {
            RimChatDetector.IsRimChatActive = true;
            var s = BridgeRimChatSettings.Get();
            s.enableActionGate = true;
            s.skipTriggerIncident = true;
            s.incidentCooldownTicks = 60000;
            SharedIncidentCooldown.RecordIncident();
            var ext = new RimChatStorytellerIncidentSkipCheck();
            Assert.True(ext.ShouldSkip(default(SkipCheckArgs)));
        }

        // ── RimChatIncidentExecutedListener ──

        [Fact]
        public void RimChatIncidentExecutedListener_Id和Owner正确()
        {
            var listener = new RimChatIncidentExecutedListener();
            Assert.Equal("rimchat_bridge_incident", listener.Id);
            Assert.Equal("RimMindBridgeRimChat", listener.OwnerModId);
        }

        [Fact]
        public void RimChatIncidentExecutedListener_OnIncidentExecuted_记录冷却()
        {
            Verse.Find.TickManager.TicksGame = 100000;
            var listener = new RimChatIncidentExecutedListener();
            listener.OnIncidentExecuted();
            Assert.True(SharedIncidentCooldown.IsOnCooldown(60000));
        }
    }
}
