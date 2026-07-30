using RimMind.Application.Common.Interfaces.Extension;
using RimMind.Bridge.RimChat;
using RimMind.Bridge.RimChat.Bridge;
using RimMind.Bridge.RimChat.Detection;
using RimMind.Bridge.RimChat.Settings;
using RimMind.Testing;
using Xunit;

namespace RimMind.Bridge.RimChat.Tests.Contracts
{
    [Collection("RimChat contracts")]
    public sealed class RimChatCompatibilityContracts
    {
        [Fact]
        public void Compatibility_defaults_preserve_safe_absent_dependency_behavior()
        {
            ContractCaseRunner.Run(
                ("settings restore the stable safe defaults", () =>
                {
                    BridgeRimChatSettings.ResetForTesting();
                    var settings = BridgeRimChatSettings.Get();
                    settings.enablePlayerInputGate = false;
                    settings.enableActionGate = false;
                    settings.forceRimMindActions = true;
                    settings.pullDiplomacyHistory = false;
                    settings.pullRpgHistory = true;

                    settings.ApplyDefaults();

                    Assert.True(settings.enablePlayerInputGate);
                    Assert.True(settings.enableActionGate);
                    Assert.False(settings.forceRimMindActions);
                    Assert.True(settings.pullDiplomacyHistory);
                    Assert.False(settings.pullRpgHistory);
                    Assert.Equal(60000, settings.incidentCooldownTicks);
                }),
                ("absent RimChat leaves every gate available", () =>
                {
                    RimChatDetector.UseActiveProbeForTesting(_ => false);
                    BridgeRimChatSettings.ResetForTesting();

                    Assert.False(DialogueGate.ShouldSkipDialogue(null, "Chitchat"));
                    Assert.False(DialogueGate.ShouldSkipFloatMenuOption());
                    Assert.False(ActionGate.ShouldSkipAction("adjust_faction"));
                    Assert.False(ActionGate.ShouldSkipStorytellerIncident());
                }),
                ("unknown reflected RimChat types remain unavailable", () =>
                {
                    RimChatApiShim.ConfigureTypesForTesting(null, null, null);
                    Assert.Null(RimChatApiShim.ApiType);
                    Assert.Null(RimChatApiShim.DiplomacyManagerType);
                    Assert.Null(RimChatApiShim.RpgArchiveManagerType);
                }));
        }

        [Fact]
        public void Extension_contracts_preserve_owner_and_skip_kinds()
        {
            ContractCaseRunner.Run(
                ("all extensions share the public owner id", () =>
                {
                    const string owner = "RimMindBridgeRimChat";

                    Assert.Equal(owner, new RimChatIncidentExecutedListener().OwnerModId);
                    Assert.Equal(owner, new RimChatActionSkipCheck().OwnerModId);
                    Assert.Equal(owner, new RimChatDialogueSkipCheck().OwnerModId);
                    Assert.Equal(owner, new RimChatFloatMenuSkipCheck().OwnerModId);
                    Assert.Equal(owner, new RimChatStorytellerIncidentSkipCheck().OwnerModId);
                    Assert.Equal(owner, new RimChatSettingsTab().OwnerModId);
                }),
                ("skip extensions expose their stable public kinds", () =>
                {
                    Assert.Equal(
                        SkipCheckKind.Action,
                        new RimChatActionSkipCheck().Kind);
                    Assert.Equal(
                        SkipCheckKind.Dialogue,
                        new RimChatDialogueSkipCheck().Kind);
                    Assert.Equal(
                        SkipCheckKind.FloatMenu,
                        new RimChatFloatMenuSkipCheck().Kind);
                    Assert.Equal(
                        SkipCheckKind.StorytellerIncident,
                        new RimChatStorytellerIncidentSkipCheck().Kind);
                }),
                ("dialogue extension delegates to the gate", () =>
                {
                    RimChatDetector.UseActiveProbeForTesting(_ => true);
                    BridgeRimChatSettings.ResetForTesting();
                    BridgeRimChatSettings.Get().enableChitchatGate = true;
                    var args = new SkipCheckArgs { Trigger = "Chitchat" };

                    Assert.True(new RimChatDialogueSkipCheck().ShouldSkip(in args));
                }),
                ("action extension keeps empty intents available", () =>
                {
                    RimChatDetector.UseActiveProbeForTesting(_ => true);
                    BridgeRimChatSettings.ResetForTesting();
                    var args = new SkipCheckArgs { IntentId = null };

                    Assert.False(new RimChatActionSkipCheck().ShouldSkip(in args));
                }));
        }
    }
}
