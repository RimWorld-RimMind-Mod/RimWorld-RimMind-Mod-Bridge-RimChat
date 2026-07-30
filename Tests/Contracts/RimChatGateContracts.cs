using RimMind.Bridge.RimChat.Bridge;
using RimMind.Bridge.RimChat.Cooldown;
using RimMind.Bridge.RimChat.Detection;
using RimMind.Bridge.RimChat.Settings;
using RimMind.Testing;
using Xunit;

namespace RimMind.Bridge.RimChat.Tests.Contracts
{
    [Collection("RimChat contracts")]
    public sealed class RimChatGateContracts
    {
        [Fact]
        public void Dialogue_gate_preserves_trigger_and_float_menu_parity()
        {
            ContractCaseRunner.Run(
                ("inactive RimChat leaves dialogue available", () =>
                {
                    Reset(active: false);
                    Assert.False(DialogueGate.ShouldSkipDialogue(null, "Chitchat"));
                    Assert.False(DialogueGate.ShouldSkipFloatMenuOption());
                }),
                ("automatic trigger switches are independent", () =>
                {
                    var settings = Reset(active: true);
                    settings.enableChitchatGate = true;
                    settings.enableAutoGate = false;

                    Assert.True(DialogueGate.ShouldSkipDialogue(null, "Chitchat"));
                    Assert.False(DialogueGate.ShouldSkipDialogue(null, "Auto"));
                    Assert.False(DialogueGate.ShouldSkipDialogue(null, "unknown"));
                }),
                ("player input and float menu share the same allow decision", () =>
                {
                    var settings = Reset(active: true);
                    settings.enablePlayerInputGate = true;
                    settings.skipPlayerDialogue = true;
                    settings.forceRimMindPlayerDialogue = false;

                    Assert.True(DialogueGate.ShouldSkipDialogue(null, "PlayerInput"));
                    Assert.True(DialogueGate.ShouldSkipFloatMenuOption());
                }),
                ("player override restores both player entry points", () =>
                {
                    var settings = Reset(active: true);
                    settings.enablePlayerInputGate = true;
                    settings.skipPlayerDialogue = true;
                    settings.forceRimMindPlayerDialogue = true;

                    Assert.False(DialogueGate.ShouldSkipDialogue(null, "PlayerInput"));
                    Assert.False(DialogueGate.ShouldSkipFloatMenuOption());
                }));
        }

        [Fact]
        public void Action_gate_preserves_classification_and_force_boundaries()
        {
            ContractCaseRunner.Run(
                ("inactive dependency and empty intent never suppress actions", () =>
                {
                    Reset(active: false);
                    Assert.False(ActionGate.ShouldSkipAction("adjust_faction"));
                    Reset(active: true);
                    Assert.False(ActionGate.ShouldSkipAction(null));
                    Assert.False(ActionGate.ShouldSkipAction(string.Empty));
                }),
                ("diplomacy social and recruit actions follow independent switches", () =>
                {
                    var settings = Reset(active: true);
                    settings.enableActionGate = true;
                    settings.skipDiplomacyActions = true;
                    settings.skipSocialActions = true;
                    settings.skipRecruitAgree = true;

                    Assert.True(ActionGate.ShouldSkipAction("adjust_faction"));
                    Assert.True(ActionGate.ShouldSkipAction("trigger_incident"));
                    Assert.True(ActionGate.ShouldSkipAction("romance_attempt"));
                    Assert.True(ActionGate.ShouldSkipAction("romance_breakup"));
                    Assert.True(ActionGate.ShouldSkipAction("recruit_agree"));
                    Assert.False(ActionGate.ShouldSkipAction("unknown"));
                }),
                ("disabled classifications remain available", () =>
                {
                    var settings = Reset(active: true);
                    settings.enableActionGate = true;
                    settings.skipDiplomacyActions = false;
                    settings.skipSocialActions = false;
                    settings.skipRecruitAgree = false;

                    Assert.False(ActionGate.ShouldSkipAction("adjust_faction"));
                    Assert.False(ActionGate.ShouldSkipAction("romance_attempt"));
                    Assert.False(ActionGate.ShouldSkipAction("recruit_agree"));
                }),
                ("force RimMind actions bypasses every action classification", () =>
                {
                    var settings = Reset(active: true);
                    settings.enableActionGate = true;
                    settings.skipDiplomacyActions = true;
                    settings.skipSocialActions = true;
                    settings.skipRecruitAgree = true;
                    settings.forceRimMindActions = true;

                    Assert.False(ActionGate.ShouldSkipAction("adjust_faction"));
                    Assert.False(ActionGate.ShouldSkipAction("romance_attempt"));
                    Assert.False(ActionGate.ShouldSkipAction("recruit_agree"));
                }));
        }

        [Fact]
        public void Incident_gate_preserves_shared_cooldown_boundaries()
        {
            ContractCaseRunner.Run(
                ("no recorded incident starts outside cooldown", () =>
                {
                    ResetCooldown(100000);
                    Assert.False(SharedIncidentCooldown.IsOnCooldown(60000));
                }),
                ("recorded incident is shared until the configured boundary", () =>
                {
                    ResetCooldown(100000);
                    SharedIncidentCooldown.RecordIncident();
                    Verse.Find.TickManager.TicksGame = 159999;
                    Assert.True(SharedIncidentCooldown.IsOnCooldown(60000));

                    Verse.Find.TickManager.TicksGame = 160000;
                    Assert.False(SharedIncidentCooldown.IsOnCooldown(60000));
                }),
                ("incident gate observes activity and skip settings", () =>
                {
                    var settings = Reset(active: true);
                    ResetCooldown(200000);
                    SharedIncidentCooldown.RecordIncident();
                    settings.enableActionGate = true;
                    settings.skipTriggerIncident = true;

                    Assert.True(ActionGate.ShouldSkipStorytellerIncident());

                    settings.skipTriggerIncident = false;
                    Assert.False(ActionGate.ShouldSkipStorytellerIncident());
                }),
                ("force action override does not disable storyteller cooldown", () =>
                {
                    var settings = Reset(active: true);
                    ResetCooldown(300000);
                    SharedIncidentCooldown.RecordIncident();
                    settings.enableActionGate = true;
                    settings.skipTriggerIncident = true;
                    settings.forceRimMindActions = true;

                    Assert.True(ActionGate.ShouldSkipStorytellerIncident());
                }),
                ("game component persists the shared per-game cooldown", () =>
                {
                    Verse.Scribe_Values.Reset();

                    new GameComponent_BridgeRimChat(new Verse.Game()).ExposeData();

                    Assert.Equal(1, Verse.Scribe_Values.Calls);
                    Assert.Equal(
                        "RimMind_BridgeRimChat_LastIncidentTick",
                        Verse.Scribe_Values.LastLabel);
                }));
        }

        private static BridgeRimChatSettings Reset(bool active)
        {
            RimChatDetector.UseActiveProbeForTesting(_ => active);
            BridgeRimChatSettings.ResetForTesting();
            return BridgeRimChatSettings.Get();
        }

        private static void ResetCooldown(int tick)
        {
            Verse.Find.TickManager.TicksGame = tick;
            SharedIncidentCooldown.ResetForTesting();
        }
    }
}
