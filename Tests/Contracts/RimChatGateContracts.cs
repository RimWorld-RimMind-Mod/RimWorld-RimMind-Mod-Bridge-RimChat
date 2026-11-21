using System.Threading;
using System.Threading.Tasks;
using RimMind.Application.Common.Constants;
using RimMind.Application.Common.Interfaces.Extension;
using RimMind.Application.Common.Interfaces.Tools;
using RimMind.Application.Common.Models.Tools;
using RimMind.Bridge.RimChat.Bridge;
using RimMind.Bridge.RimChat.Cooldown;
using RimMind.Bridge.RimChat.Detection;
using RimMind.Bridge.RimChat.Settings;
using RimMind.Domain.Agent.Policies;
using RimMind.Domain.ValueObjects;
using RimMind.Presentation.Api;
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
                }),
                ("shared DialogueTriggerKinds constants match DialogueGate switch arms", () =>
                {
                    var settings = Reset(active: true);
                    settings.enableChitchatGate = true;
                    settings.enableAutoGate = true;
                    settings.enablePlayerInputGate = true;
                    settings.skipPlayerDialogue = true;
                    settings.forceRimMindPlayerDialogue = false;

                    Assert.True(DialogueGate.ShouldSkipDialogue(null, DialogueTriggerKinds.Chitchat));
                    Assert.True(DialogueGate.ShouldSkipDialogue(null, DialogueTriggerKinds.Auto));
                    Assert.True(DialogueGate.ShouldSkipDialogue(null, DialogueTriggerKinds.PlayerInput));
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
                }),
                ("real mechanism and tool prefixes match romance social recruit and diplomacy gates", () =>
                {
                    var settings = Reset(active: true);
                    settings.enableActionGate = true;

                    settings.skipSocialActions = true;
                    settings.skipDiplomacyActions = false;
                    settings.skipRecruitAgree = false;
                    Assert.True(ActionGate.IsRomanceAction("romance_attempt"));
                    Assert.True(ActionGate.IsRomanceAction("pawn.interaction.romance_attempt"));
                    Assert.True(ActionGate.IsRomanceAction("pawn.interaction.romance_breakup"));
                    Assert.True(ActionGate.IsRomanceAction("custom_romance_tag"));
                    Assert.True(ActionGate.ShouldSkipAction("pawn.interaction"));
                    Assert.True(ActionGate.ShouldSkipAction("pawn.interaction.social_relax"));
                    Assert.True(ActionGate.ShouldSkipAction("pawn.interaction.give_item"));
                    Assert.True(ActionGate.ShouldSkipAction("pawn.interaction.romance_attempt"));
                    Assert.False(ActionGate.ShouldSkipAction("world.faction"));

                    settings.skipSocialActions = false;
                    settings.skipDiplomacyActions = true;
                    Assert.True(ActionGate.ShouldSkipAction("world.faction"));
                    Assert.True(ActionGate.ShouldSkipAction("world.faction.adjust_goodwill"));
                    Assert.True(ActionGate.ShouldSkipAction("faction.goodwill"));
                    Assert.True(ActionGate.ShouldSkipAction("diplomacy"));
                    Assert.True(ActionGate.ShouldSkipAction("storyteller.incident"));
                    Assert.True(ActionGate.ShouldSkipAction("incident.raid"));
                    Assert.False(ActionGate.ShouldSkipAction("pawn.interaction"));

                    settings.skipDiplomacyActions = false;
                    settings.skipRecruitAgree = true;
                    Assert.True(ActionGate.ShouldSkipAction("recruit"));
                    Assert.True(ActionGate.ShouldSkipAction("pawn.recruit"));
                    Assert.True(ActionGate.ShouldSkipAction("prisoner.recruit"));
                    Assert.True(ActionGate.ShouldSkipAction("prisoner.recruit.chat"));
                }),
                ("skip check extensions expose open string channels", () =>
                {
                    Assert.Equal(SkipCheckChannels.Action, new RimChatActionSkipCheck().Channel);
                    Assert.Equal(SkipCheckChannels.Dialogue, new RimChatDialogueSkipCheck().Channel);
                    Assert.Equal(SkipCheckChannels.FloatMenu, new RimChatFloatMenuSkipCheck().Channel);
                    Assert.Equal(SkipCheckChannels.StorytellerIncident, new RimChatStorytellerIncidentSkipCheck().Channel);
                }),
                ("ToolManifest scopes, kind, and autonomy domain drive classification without hardcoded tool names", () =>
                {
                    var settings = Reset(active: true);
                    settings.enableActionGate = true;
                    settings.skipDiplomacyActions = true;
                    settings.skipSocialActions = true;
                    settings.skipRecruitAgree = true;

                    // Custom non-keyword tools classified purely by ToolManifest
                    RegisterProbeTool("custom_treaty_offer", new ToolManifest
                    {
                        Kind = ToolKind.Mutation,
                        Scopes = ToolScope.WorldAdmin
                    });
                    RegisterProbeTool("custom_bond_chat", new ToolManifest
                    {
                        Kind = ToolKind.Mutation,
                        Scopes = ToolScope.Social
                    });
                    RegisterProbeTool("custom_convert_guest", new ToolManifest
                    {
                        Kind = ToolKind.Mutation,
                        Scopes = ToolScope.Social,
                        AutonomyDomainId = "social.recruit"
                    });
                    RegisterProbeTool("custom_flirt_gesture", new ToolManifest
                    {
                        Kind = ToolKind.Mutation,
                        Scopes = ToolScope.Social,
                        AutonomyDomainId = "romance"
                    });
                    // Read-only Query tools are never gated even if scoped to WorldAdmin or Social
                    RegisterProbeTool("world.faction.query", new ToolManifest
                    {
                        Kind = ToolKind.Query,
                        Scopes = ToolScope.WorldAdmin
                    });
                    RegisterProbeTool("pawn.relations.query", new ToolManifest
                    {
                        Kind = ToolKind.Query,
                        Scopes = ToolScope.Social
                    });
                    // Explicit non-matching ColonyBuild manifest overrides accidental keyword in tool id
                    RegisterProbeTool("build_faction_statue", new ToolManifest
                    {
                        Kind = ToolKind.Mutation,
                        Scopes = ToolScope.ColonyBuild,
                        DefaultDomain = AutonomyDomain.BlueprintPlacement
                    });
                    // Core mechanism recruit tool stays isolated under Recruit, not Social
                    RegisterProbeTool("pawn.recruit.trigger", new ToolManifest
                    {
                        Kind = ToolKind.Mutation,
                        Scopes = ToolScope.Social | ToolScope.ColonistSelf
                    });

                    Assert.True(ActionGate.IsDiplomacyAction("custom_treaty_offer"));
                    Assert.False(ActionGate.IsSocialAction("custom_treaty_offer"));

                    Assert.True(ActionGate.IsSocialAction("custom_bond_chat"));
                    Assert.False(ActionGate.IsDiplomacyAction("custom_bond_chat"));
                    Assert.False(ActionGate.IsRecruitAction("custom_bond_chat"));

                    Assert.True(ActionGate.IsRecruitAction("custom_convert_guest"));
                    Assert.False(ActionGate.IsSocialAction("custom_convert_guest"));

                    Assert.True(ActionGate.IsRomanceAction("custom_flirt_gesture"));
                    Assert.True(ActionGate.IsSocialAction("custom_flirt_gesture"));

                    Assert.False(ActionGate.ShouldSkipAction("world.faction.query"));
                    Assert.False(ActionGate.ShouldSkipAction("pawn.relations.query"));
                    Assert.False(ActionGate.ShouldSkipAction("build_faction_statue"));

                    settings.skipSocialActions = true;
                    settings.skipRecruitAgree = false;
                    Assert.False(ActionGate.ShouldSkipAction("pawn.recruit.trigger"));
                    Assert.True(ActionGate.ShouldSkipAction("custom_bond_chat"));

                    settings.skipSocialActions = false;
                    settings.skipRecruitAgree = true;
                    Assert.True(ActionGate.ShouldSkipAction("pawn.recruit.trigger"));
                    Assert.True(ActionGate.ShouldSkipAction("custom_convert_guest"));
                    Assert.False(ActionGate.ShouldSkipAction("custom_bond_chat"));
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
                ("detector caches active probe for 6000 ticks and listener respects gate settings", () =>
                {
                    int probeCount = 0;
                    Verse.Find.TickManager.TicksGame = 1000;
                    RimChatDetector.UseActiveProbeForTesting(_ =>
                    {
                        probeCount++;
                        return true;
                    });

                    Assert.True(RimChatDetector.IsRimChatActive);
                    Assert.True(RimChatDetector.IsRimChatActive);
                    Verse.Find.TickManager.TicksGame = 6999;
                    Assert.True(RimChatDetector.IsRimChatActive);
                    Assert.Equal(1, probeCount);

                    Verse.Find.TickManager.TicksGame = 7001;
                    Assert.True(RimChatDetector.IsRimChatActive);
                    Assert.Equal(2, probeCount);

                    ResetCooldown(10000);
                    var settings = BridgeRimChatSettings.Get();
                    settings.enableActionGate = true;
                    settings.skipTriggerIncident = false;
                    var listener = new RimChatIncidentExecutedListener();
                    listener.OnIncidentExecuted();
                    Assert.False(SharedIncidentCooldown.IsOnCooldown(60000));

                    settings.skipTriggerIncident = true;
                    listener.OnIncidentExecuted();
                    Assert.True(SharedIncidentCooldown.IsOnCooldown(60000));
                }));
        }

        private static BridgeRimChatSettings Reset(bool active)
        {
            RimChatDetector.UseActiveProbeForTesting(_ => active);
            BridgeRimChatSettings.ResetForTesting();
            RimMindAPI.ToolSet.Reset();
            return BridgeRimChatSettings.Get();
        }

        private static void RegisterProbeTool(string id, ToolManifest manifest)
        {
            RimMindAPI.ToolSet.Registry.Register(new ProbeToolHandler(new ToolDefinition
            {
                Id = id,
                Manifest = manifest
            }));
        }

        private sealed class ProbeToolHandler : IToolHandler
        {
            public ProbeToolHandler(ToolDefinition definition) => Definition = definition;
            public string Id => Definition.Id;
            public string OwnerModId => "TestMod";
            public ToolDefinition Definition { get; }
            public Task<Result<ToolResult, RimMindError>> ExecuteAsync(ToolCallArgs args, CancellationToken ct)
                => Task.FromResult(Result<ToolResult, RimMindError>.Ok(ToolResult.Ok("ok")));
        }

        private static void ResetCooldown(int tick)
        {
            Verse.Find.TickManager.TicksGame = tick;
            StartGame();
        }

        [Fact]
        public void Incident_cooldown_does_not_cross_game_lifetimes()
        {
            ResetCooldown(300000);
            SharedIncidentCooldown.RecordIncident();
            Assert.True(SharedIncidentCooldown.IsOnCooldown(60000));

            StartGame();
            Verse.Find.TickManager.TicksGame = 0;
            Assert.False(SharedIncidentCooldown.IsOnCooldown(60000));

            SharedIncidentCooldown.RecordIncident();
            Assert.True(SharedIncidentCooldown.IsOnCooldown(60000));
            Verse.Current.Game = null;
            Assert.False(SharedIncidentCooldown.IsOnCooldown(60000));
        }

        [Fact]
        public void Saved_incident_cooldown_restores_only_into_the_loaded_game()
        {
            ResetCooldown(100000);
            SharedIncidentCooldown.RecordIncident();
            Verse.Scribe_Values.Reset();
            Verse.Current.Game!.GetComponent<GameComponent_BridgeRimChat>()!.ExposeData();
            Assert.Equal("RimMind_BridgeRimChat_LastIncidentTick", Verse.Scribe_Values.LastLabel);

            StartGame();
            Verse.Find.TickManager.TicksGame = 159999;
            Assert.False(SharedIncidentCooldown.IsOnCooldown(60000));

            Verse.Scribe_Values.Loading = true;
            Verse.Current.Game!.GetComponent<GameComponent_BridgeRimChat>()!.ExposeData();
            Verse.Scribe_Values.Loading = false;
            Assert.True(SharedIncidentCooldown.IsOnCooldown(60000));
            Verse.Find.TickManager.TicksGame = 160000;
            Assert.False(SharedIncidentCooldown.IsOnCooldown(60000));
        }

        private static void StartGame()
        {
            var game = new Verse.Game();
            game.Components.Add(new GameComponent_BridgeRimChat(game));
            Verse.Current.Game = game;
        }
    }
}
