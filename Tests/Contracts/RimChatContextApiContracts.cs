using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection;
using System.Threading;
using RimMind.Application.Common.Interfaces.Context;
using RimMind.Bridge.RimChat.Bridge;
using RimMind.Bridge.RimChat.Detection;
using RimMind.Bridge.RimChat.Settings;
using RimMind.Testing;
using Xunit;

namespace RimMind.Bridge.RimChat.Tests.Contracts
{
    [Collection("RimChat contracts")]
    public sealed class RimChatContextApiContracts
    {
        [Fact]
        public void Context_exposure_preserves_registration_and_owner_metadata()
        {
            ContractCaseRunner.Run(
                ("inactive dependency registers no context", () =>
                {
                    ResetContext(active: false);

                    ContextPullBridge.Register();

                    Assert.Empty(RimMind.Presentation.Api.RimMindAPI.Context.ContextKeys.GetAll());
                }),
                ("enabled diplomacy and RPG histories register both public keys", () =>
                {
                    var settings = ResetContext(active: true);
                    settings.enableContextPull = true;
                    settings.pullDiplomacyHistory = true;
                    settings.pullRpgHistory = true;

                    ContextPullBridge.Register();

                    Assert.Collection(
                        RimMind.Presentation.Api.RimMindAPI.Context.ContextKeys.GetAll(),
                        meta =>
                        {
                            Assert.Equal("rimchat_diplomacy", meta.Key);
                            Assert.Equal("RimMindBridgeRimChat", meta.OwnerMod);
                        },
                        meta =>
                        {
                            Assert.Equal("rimchat_rpg_history", meta.Key);
                            Assert.Equal("RimMindBridgeRimChat", meta.OwnerMod);
                        });
                }),
                ("individual history switches register only their boundary", () =>
                {
                    var settings = ResetContext(active: true);
                    settings.enableContextPull = true;
                    settings.pullDiplomacyHistory = false;
                    settings.pullRpgHistory = true;

                    ContextPullBridge.Register();

                    Assert.Equal(
                        "rimchat_rpg_history",
                        Assert.Single(
                            RimMind.Presentation.Api.RimMindAPI.Context.ContextKeys.GetAll()).Key);
                }),
                ("unregister removes both public keys", () =>
                {
                    var settings = ResetContext(active: true);
                    settings.pullDiplomacyHistory = true;
                    settings.pullRpgHistory = true;
                    ContextPullBridge.Register();

                    ContextPullBridge.Unregister();

                    Assert.Empty(RimMind.Presentation.Api.RimMindAPI.Context.ContextKeys.GetAll());
                }),
                ("registered diplomacy provider projects bounded real history", async () =>
                {
                    BridgeRimChatSettings settings = ResetContext(active: true);
                    settings.pullDiplomacyHistory = true;
                    settings.pullRpgHistory = false;
                    DiplomacyManagerProbe.Instance.dialogueSessions.Clear();
                    DiplomacyManagerProbe.Instance.dialogueSessions.Add(
                        new DiplomacySessionProbe
                        {
                            faction = "Outlanders",
                            messages = new ArrayList
                            {
                                new DiplomacyMessageProbe { sender = "old", message = "discarded" },
                                new DiplomacyMessageProbe { sender = "Lia", message = "one" },
                                new DiplomacyMessageProbe { sender = "Lia", message = "two" },
                                new DiplomacyMessageProbe { sender = "Lia", message = new string('x', 130) },
                                new DiplomacyMessageProbe { sender = "player", message = "four", isPlayer = true }
                            }
                        });
                    RimChatApiShim.ConfigureTypesForTesting(
                        apiType: null,
                        diplomacyManagerType: typeof(DiplomacyManagerProbe),
                        rpgArchiveManagerType: null);
                    ContextPullBridge.Register();

                    var meta = Assert.Single(
                        RimMind.Presentation.Api.RimMindAPI.Context.ContextKeys.GetAll());
                    var def = Assert.IsType<ContextProviderDef>(meta.Def);
                    string? projected = await def.Provider(
                        new ProviderContext("contract", "trace"),
                        CancellationToken.None);

                    Assert.Contains("[RimChat Diplomacy]", projected);
                    Assert.Contains("## Outlanders", projected);
                    Assert.DoesNotContain("discarded", projected);
                    Assert.Contains("[Player] four", projected);
                    Assert.Contains("...", projected);
                }));
        }

        [Fact]
        public void Pawn_lookup_preserves_world_and_all_map_boundaries()
        {
            ContractCaseRunner.Run(
                ("world pawns are resolved before map pawns", () =>
                {
                    ResetPawnSources();
                    var worldPawn = new Verse.Pawn { thingIDNumber = 41 };
                    Verse.Find.WorldPawns.AllPawnsAlive.Add(worldPawn);
                    Verse.Find.Maps.Add(MapWith(new Verse.Pawn { thingIDNumber = 41 }));

                    Assert.Same(worldPawn, ContextPullBridge.TryFindPawnById(41));
                }),
                ("colonists on non-current maps remain visible", () =>
                {
                    ResetPawnSources();
                    var otherMapPawn = new Verse.Pawn { thingIDNumber = 42 };
                    Verse.Find.CurrentMap = MapWith(new Verse.Pawn { thingIDNumber = 1 });
                    Verse.Find.Maps.Add(Verse.Find.CurrentMap);
                    Verse.Find.Maps.Add(MapWith(otherMapPawn));

                    Assert.Same(otherMapPawn, ContextPullBridge.TryFindPawnById(42));
                }),
                ("unknown pawn id has no projected context target", () =>
                {
                    ResetPawnSources();
                    Assert.Null(ContextPullBridge.TryFindPawnById(99));
                }));
        }

        [Fact]
        public void API_shim_preserves_reflection_success_and_failure_contracts()
        {
            ContractCaseRunner.Run(
                ("static property and manager singleton are exposed", () =>
                {
                    Assert.Equal(
                        "value",
                        RimChatApiShim.GetStaticPropertyValue(
                            typeof(ManagerProbe),
                            "StaticValue"));
                    Assert.Same(
                        ManagerProbe.Instance,
                        RimChatApiShim.TryGetManagerInstance(typeof(ManagerProbe)));
                }),
                ("public and private instance fields honor binding flags", () =>
                {
                    var probe = new FieldProbe();

                    Assert.Equal(
                        "public",
                        RimChatApiShim.GetInstanceFieldValue(probe, "PublicValue"));
                    Assert.Equal(
                        "private",
                        RimChatApiShim.GetInstanceFieldValue(
                            probe,
                            "_privateValue",
                            BindingFlags.NonPublic | BindingFlags.Instance));
                }),
                ("missing reflection members fail closed", () =>
                {
                    var probe = new FieldProbe();

                    Assert.Null(RimChatApiShim.GetStaticPropertyValue(
                        typeof(ManagerProbe),
                        "Missing"));
                    Assert.Null(RimChatApiShim.GetInstanceFieldValue(probe, "Missing"));
                    Assert.Null(RimChatApiShim.TryGetManagerInstance(null));
                }));
        }

        private static BridgeRimChatSettings ResetContext(bool active)
        {
            RimChatDetector.UseActiveProbeForTesting(_ => active);
            BridgeRimChatSettings.ResetForTesting();
            RimMind.Presentation.Api.RimMindAPI.ResetCounts();
            RimMind.Presentation.Api.RimMindAPI.Context.ContextKeys.Clear();
            return BridgeRimChatSettings.Get();
        }

        private static void ResetPawnSources()
        {
            Verse.Find.WorldPawns = new Verse.WorldPawnsHolder();
            Verse.Find.Maps.Clear();
            Verse.Find.CurrentMap = null;
        }

        private static Verse.MapHolder MapWith(Verse.Pawn pawn)
        {
            return new Verse.MapHolder
            {
                mapPawns = new Verse.MapPawnsHolder
                {
                    FreeColonists = { pawn }
                }
            };
        }

        private sealed class ManagerProbe
        {
            public static string StaticValue => "value";
            public static ManagerProbe Instance { get; } = new ManagerProbe();
        }

        private sealed class FieldProbe
        {
            public string PublicValue = "public";
#pragma warning disable CS0414
            private string _privateValue = "private";
#pragma warning restore CS0414
        }

        private sealed class DiplomacyManagerProbe
        {
            public static DiplomacyManagerProbe Instance { get; } =
                new DiplomacyManagerProbe();

            public IList dialogueSessions = new ArrayList();
        }

        private sealed class DiplomacySessionProbe
        {
            public object faction = string.Empty;
            public IList messages = new ArrayList();
        }

        private sealed class DiplomacyMessageProbe
        {
            public string sender = string.Empty;
            public string message = string.Empty;
            public bool isPlayer;
        }
    }
}
