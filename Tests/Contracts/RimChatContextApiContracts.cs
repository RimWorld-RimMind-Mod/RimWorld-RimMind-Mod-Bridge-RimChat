using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using RimMind.Application.Common.Interfaces.Context;
using RimMind.Application.Common.Interfaces.Extension;
using RimMind.Bridge.RimChat.Bridge;
using RimMind.Bridge.RimChat.Detection;
using RimMind.Bridge.RimChat.Settings;
using RimMind.Presentation.Api;
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

                    var bridge = new ContextPullBridge();
                    bridge.Register();

                    Assert.False(bridge.IsRegistered);
                    Assert.Empty(RimMindAPI.Context.ContextKeys.GetAll());
                }),
                ("enabled diplomacy and RPG histories register both public keys", () =>
                {
                    var settings = ResetContext(active: true);
                    settings.enableContextPull = true;
                    settings.pullDiplomacyHistory = true;
                    settings.pullRpgHistory = true;

                    var bridge = new ContextPullBridge();
                    bridge.Register();

                    Assert.True(bridge.IsRegistered);
                    Assert.Equal("context_pull", bridge.Id);
                    Assert.Equal("RimMindBridgeRimChat", bridge.OwnerModId);
                    Assert.Collection(
                        RimMindAPI.Context.ContextKeys.GetAll(),
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

                    var bridge = new ContextPullBridge();
                    bridge.Register();

                    Assert.True(bridge.IsRegistered);
                    Assert.Equal(
                        "rimchat_rpg_history",
                        Assert.Single(
                            RimMindAPI.Context.ContextKeys.GetAll()).Key);
                }),
                ("unregister removes both public keys symmetrically", () =>
                {
                    var settings = ResetContext(active: true);
                    settings.pullDiplomacyHistory = true;
                    settings.pullRpgHistory = true;
                    var bridge = new ContextPullBridge();
                    bridge.Register();

                    bridge.Unregister();

                    Assert.False(bridge.IsRegistered);
                    Assert.Empty(RimMindAPI.Context.ContextKeys.GetAll());
                }),
                ("RimChatBridgeCoordinator manages gates, incident listener, and context pull symmetrically", () =>
                {
                    var settings = ResetContext(active: false);
                    RimChatBridgeCoordinator.ResetForTesting();
                    RimMindAPI.Ext.Reset();

                    RimChatBridgeCoordinator.Register();
                    Assert.Empty(RimMindAPI.Ext.Get<ISkipCheck>().All);
                    Assert.Empty(RimMindAPI.Ext.Get<IIncidentExecutedListener>().All);
                    Assert.Empty(RimMindAPI.Context.ContextKeys.GetAll());

                    settings = ResetContext(active: true);
                    settings.pullDiplomacyHistory = true;
                    settings.pullRpgHistory = false;
                    RimChatBridgeCoordinator.ResetForTesting();
                    RimMindAPI.Ext.Reset();

                    RimChatBridgeCoordinator.Register();

                    Assert.Equal(
                        new[]
                        {
                            "rimchat_bridge_dialogue",
                            "rimchat_bridge_floatmenu",
                            "rimchat_bridge_action",
                            "rimchat_bridge_storyteller"
                        },
                        RimMindAPI.Ext.Get<ISkipCheck>().All.Select(g => g.Id));
                    Assert.Single(RimMindAPI.Ext.Get<IIncidentExecutedListener>().All);
                    Assert.All(RimChatBridgeCoordinator.Modules, m => Assert.True(m.IsRegistered));
                    Assert.Equal("rimchat_diplomacy", Assert.Single(RimMindAPI.Context.ContextKeys.GetAll()).Key);

                    // RefreshContextPull updates only providers without re-registering skip checks
                    int beforeCount = RimMindAPI.ExtensionRegisterCount;
                    settings.pullDiplomacyHistory = false;
                    settings.pullRpgHistory = true;
                    RimChatBridgeCoordinator.RefreshContextPull();
                    Assert.Equal(beforeCount, RimMindAPI.ExtensionRegisterCount);
                    Assert.Equal("rimchat_rpg_history", Assert.Single(RimMindAPI.Context.ContextKeys.GetAll()).Key);

                    RimChatBridgeCoordinator.Unregister();

                    Assert.Empty(RimMindAPI.Ext.Get<ISkipCheck>().All);
                    Assert.Empty(RimMindAPI.Ext.Get<IIncidentExecutedListener>().All);
                    Assert.All(RimChatBridgeCoordinator.Modules, m => Assert.False(m.IsRegistered));
                    Assert.Empty(RimMindAPI.Context.ContextKeys.GetAll());

                    RimChatBridgeCoordinator.ResetForTesting();
                }),
                ("registered diplomacy provider projects most recent 3 sessions and separates header with newline", () =>
                {
                    BridgeRimChatSettings settings = ResetContext(active: true);
                    settings.pullDiplomacyHistory = true;
                    settings.pullRpgHistory = false;
                    DiplomacyManagerProbe.Instance.dialogueSessions.Clear();
                    DiplomacyManagerProbe.Instance.dialogueSessions.Add(
                        new DiplomacySessionProbe
                        {
                            faction = "OldestFaction",
                            messages = new ArrayList
                            {
                                new DiplomacyMessageProbe { sender = "Old", message = "should be skipped" }
                            }
                        });
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
                    DiplomacyManagerProbe.Instance.dialogueSessions.Add(
                        new DiplomacySessionProbe
                        {
                            faction = "Empire",
                            messages = new ArrayList
                            {
                                new DiplomacyMessageProbe { sender = "Consul", message = "greetings" }
                            }
                        });
                    DiplomacyManagerProbe.Instance.dialogueSessions.Add(
                        new DiplomacySessionProbe
                        {
                            faction = "Tribe",
                            messages = new ArrayList
                            {
                                new DiplomacyMessageProbe { sender = "Chief", message = "peace" }
                            }
                        });
                    RimChatApiShim.ConfigureTypesForTesting(
                        apiType: null,
                        diplomacyManagerType: typeof(DiplomacyManagerProbe),
                        rpgArchiveManagerType: null);
                    var bridge = new ContextPullBridge();
                    bridge.Register();

                    var meta = Assert.Single(
                        RimMindAPI.Context.ContextKeys.GetAll());
                    var def = Assert.IsType<ContextProviderDef>(meta.Def);
                    string? projected = def.Provider(
                        new ProviderContext("contract", "trace"),
                        CancellationToken.None).GetAwaiter().GetResult();

                    Assert.NotNull(projected);
                    Assert.StartsWith("[RimChat Diplomacy]" + Environment.NewLine + "## Outlanders", projected);
                    Assert.DoesNotContain("OldestFaction", projected);
                    Assert.Contains("## Outlanders", projected);
                    Assert.Contains("## Empire", projected);
                    Assert.Contains("## Tribe", projected);
                    Assert.DoesNotContain("discarded", projected);
                    Assert.Contains("[Player] four", projected);
                    Assert.Contains("...", projected);
                }));
        }

        [Fact]
        public void Pawn_lookup_preserves_world_and_all_map_boundaries()
        {
            ContractCaseRunner.Run(
                ("active map colonists are resolved without scanning world pawns", () =>
                {
                    ResetPawnSources();
                    var worldPawn = new Verse.Pawn { thingIDNumber = 41 };
                    var mapPawn = new Verse.Pawn { thingIDNumber = 41 };
                    Verse.Find.WorldPawns.AllPawnsAlive.Add(worldPawn);
                    Verse.Find.Maps.Add(MapWith(mapPawn));

                    Assert.Same(mapPawn, ContextPullBridge.TryFindPawnById(41));
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
                ("inactive or prisoner pawns are rejected", () =>
                {
                    ResetPawnSources();
                    var deadPawn = new Verse.Pawn { thingIDNumber = 50, Dead = true };
                    var prisonerPawn = new Verse.Pawn { thingIDNumber = 51, IsPrisoner = true };
                    Verse.Find.Maps.Add(MapWith(deadPawn));
                    Verse.Find.Maps.Add(MapWith(prisonerPawn));

                    Assert.Null(ContextPullBridge.TryFindPawnById(50));
                    Assert.Null(ContextPullBridge.TryFindPawnById(51));
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
                ("public and private instance fields honor binding flags and cache FieldInfo", () =>
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
                    Assert.Same(
                        RimChatApiShim.GetCachedField(typeof(FieldProbe), "PublicValue"),
                        RimChatApiShim.GetCachedField(typeof(FieldProbe), "PublicValue"));
                    Assert.Same(
                        RimChatApiShim.GetCachedMethod(typeof(ManagerProbe), "Ping"),
                        RimChatApiShim.GetCachedMethod(typeof(ManagerProbe), "Ping"));
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
            public static string Ping() => "pong";
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
