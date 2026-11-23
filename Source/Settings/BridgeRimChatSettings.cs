using RimMind.Bridge.RimChat.Bridge;
using RimMind.Presentation.Api;
using RimMind.Presentation.UI;
using UnityEngine;
using Verse;

namespace RimMind.Bridge.RimChat.Settings
{
    public partial class BridgeRimChatSettings : ModSettings
    {
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref enablePlayerInputGate, "enablePlayerInputGate", true);
            Scribe_Values.Look(ref enableChitchatGate, "enableChitchatGate", true);
            Scribe_Values.Look(ref enableAutoGate, "enableAutoGate", true);
            Scribe_Values.Look(ref skipPlayerDialogue, "skipPlayerDialogue", true);
            Scribe_Values.Look(ref forceRimMindPlayerDialogue, "forceRimMindPlayerDialogue", false);

            Scribe_Values.Look(ref enableActionGate, "enableActionGate", true);
            Scribe_Values.Look(ref skipDiplomacyActions, "skipDiplomacyActions", true);
            Scribe_Values.Look(ref skipTriggerIncident, "skipTriggerIncident", true);
            Scribe_Values.Look(ref skipSocialActions, "skipSocialActions", false);
            Scribe_Values.Look(ref skipRecruitAgree, "skipRecruitAgree", false);
            Scribe_Values.Look(ref incidentCooldownTicks, "incidentCooldownTicks", 60000);
            Scribe_Values.Look(ref forceRimMindActions, "forceRimMindActions", false);

            Scribe_Values.Look(ref enableContextPull, "enableContextPull", true);
            Scribe_Values.Look(ref pullDiplomacyHistory, "pullDiplomacyHistory", true);
            Scribe_Values.Look(ref pullRpgHistory, "pullRpgHistory", false);
        }

        private static Vector2 _scrollPos = Vector2.zero;
        private static float _measuredHeight;

        public static void DrawSettingsContent(Rect inRect)
        {
            var s = Get();

            bool oldEnableContextPull = s.enableContextPull;
            bool oldPullDiplomacy = s.pullDiplomacyHistory;
            bool oldPullRpg = s.pullRpgHistory;

            SettingsUIDrawer.DrawScrollableSettingsPage(
                inRect,
                ref _scrollPos,
                ref _measuredHeight,
                listing => DrawSettingsBody(listing, s),
                onReset: () => s.ApplyDefaults(),
                onDirtySave: () => s.Write());

            if (s.enableContextPull != oldEnableContextPull
                || s.pullDiplomacyHistory != oldPullDiplomacy
                || s.pullRpgHistory != oldPullRpg)
            {
                RimChatBridgeCoordinator.RefreshContextPull();
            }
        }

        private static void DrawSettingsBody(Listing_Standard listing, BridgeRimChatSettings s)
        {
            SettingsUIDrawer.DrawSectionHeader(
                listing,
                RimMindAPI.Text.TrOr("RimMind.BridgeRimChat.Settings.Section.DialogueGate", "Dialogue Gate"));
            listing.CheckboxLabeled(
                RimMindAPI.Text.TrOr("RimMind.BridgeRimChat.Settings.EnablePlayerInputGate", "Gate player dialogue"),
                ref s.enablePlayerInputGate,
                RimMindAPI.Text.TrOr(
                    "RimMind.BridgeRimChat.Settings.EnablePlayerInputGate.Desc",
                    "When RimChat is present, skip RimMind-Dialogue's player dialogue entry to avoid duplicate chat interfaces"));
            if (s.enablePlayerInputGate)
            {
                listing.CheckboxLabeled(
                    "  " + RimMindAPI.Text.TrOr("RimMind.BridgeRimChat.Settings.SkipPlayerDialogue", "Skip player dialogue"),
                    ref s.skipPlayerDialogue,
                    RimMindAPI.Text.TrOr(
                        "RimMind.BridgeRimChat.Settings.SkipPlayerDialogue.Desc",
                        "When RimChat is present, skip RimMind's player dialogue triggers and remove the \"Chat with X\" float menu option"));
                if (s.skipPlayerDialogue)
                {
                    listing.CheckboxLabeled(
                        "    " + RimMindAPI.Text.TrOr(
                            "RimMind.BridgeRimChat.Settings.ForceRimMindPlayerDialogue",
                            "Keep RimMind player dialogue"),
                        ref s.forceRimMindPlayerDialogue,
                        RimMindAPI.Text.TrOr(
                            "RimMind.BridgeRimChat.Settings.ForceRimMindPlayerDialogue.Desc",
                            "Keep RimMind's player dialogue triggers and float menu option even when skipping, allowing both chat systems to coexist"));
                }
            }
            listing.CheckboxLabeled(
                RimMindAPI.Text.TrOr("RimMind.BridgeRimChat.Settings.EnableChitchatGate", "Gate chitchat triggers"),
                ref s.enableChitchatGate,
                RimMindAPI.Text.TrOr(
                    "RimMind.BridgeRimChat.Settings.EnableChitchatGate.Desc",
                    "When RimChat is present, skip RimMind-Dialogue's chitchat triggers to avoid duplicate conversations"));
            listing.CheckboxLabeled(
                RimMindAPI.Text.TrOr("RimMind.BridgeRimChat.Settings.EnableAutoGate", "Gate auto dialogue"),
                ref s.enableAutoGate,
                RimMindAPI.Text.TrOr(
                    "RimMind.BridgeRimChat.Settings.EnableAutoGate.Desc",
                    "When RimChat is present, skip RimMind-Dialogue's auto dialogue triggers to avoid duplicate conversations"));

            SettingsUIDrawer.DrawSectionHeader(
                listing,
                RimMindAPI.Text.TrOr("RimMind.BridgeRimChat.Settings.Section.ActionGate", "Action Gate"));
            listing.CheckboxLabeled(
                RimMindAPI.Text.TrOr("RimMind.BridgeRimChat.Settings.EnableActionGate", "Enable action gate"),
                ref s.enableActionGate,
                RimMindAPI.Text.TrOr(
                    "RimMind.BridgeRimChat.Settings.EnableActionGate.Desc",
                    "When RimChat is present, skip RimMind-Actions and Storyteller incidents that overlap with RimChat's capabilities to avoid duplicate execution"));
            if (s.enableActionGate)
            {
                listing.CheckboxLabeled(
                    "  " + RimMindAPI.Text.TrOr("RimMind.BridgeRimChat.Settings.SkipDiplomacyActions", "Skip diplomacy actions"),
                    ref s.skipDiplomacyActions,
                    RimMindAPI.Text.TrOr(
                        "RimMind.BridgeRimChat.Settings.SkipDiplomacyActions.Desc",
                        "Skip adjust_faction and trigger_incident actions, handled by RimChat's diplomacy system"));
                listing.CheckboxLabeled(
                    "  " + RimMindAPI.Text.TrOr("RimMind.BridgeRimChat.Settings.SkipTriggerIncident", "Skip Storyteller incident"),
                    ref s.skipTriggerIncident,
                    RimMindAPI.Text.TrOr(
                        "RimMind.BridgeRimChat.Settings.SkipTriggerIncident.Desc",
                        "Skip RimMind-Storyteller incident triggers to avoid duplicate event firing with RimChat"));
                listing.CheckboxLabeled(
                    "  " + RimMindAPI.Text.TrOr("RimMind.BridgeRimChat.Settings.SkipSocialActions", "Skip social actions"),
                    ref s.skipSocialActions,
                    RimMindAPI.Text.TrOr(
                        "RimMind.BridgeRimChat.Settings.SkipSocialActions.Desc",
                        "Skip romance_attempt and romance_breakup actions"));
                listing.CheckboxLabeled(
                    "  " + RimMindAPI.Text.TrOr("RimMind.BridgeRimChat.Settings.SkipRecruitAgree", "Skip recruit action"),
                    ref s.skipRecruitAgree,
                    RimMindAPI.Text.TrOr(
                        "RimMind.BridgeRimChat.Settings.SkipRecruitAgree.Desc",
                        "Skip recruit_agree action, let RimChat handle recruitment decisions"));
                if (s.skipTriggerIncident)
                {
                    string incidentTip = RimMindAPI.Text.TrOr(
                        "RimMind.BridgeRimChat.Settings.IncidentCooldown.Desc",
                        "Minimum cooldown between two Storyteller incident triggers, preventing repeated events in a short time.");
                    string cooldownLabel = RimMindAPI.Text.TrOr(
                        "RimMind.BridgeRimChat.Settings.IncidentCooldown",
                        "Incident cooldown: {0} game days",
                        $"{s.incidentCooldownTicks / 60000f:F1}");
                    listing.LabelWithTooltip("  " + cooldownLabel, incidentTip);
                    float rawCooldown = listing.SliderWithTooltip(s.incidentCooldownTicks, 6000f, 180000f, incidentTip);
                    s.incidentCooldownTicks = Mathf.RoundToInt(rawCooldown / 1500f) * 1500;
                }
                listing.CheckboxLabeled(
                    RimMindAPI.Text.TrOr("RimMind.BridgeRimChat.Settings.ForceRimMindActions", "Force RimMind to execute all actions"),
                    ref s.forceRimMindActions,
                    RimMindAPI.Text.TrOr(
                        "RimMind.BridgeRimChat.Settings.ForceRimMindActions.Desc",
                        "Ignore mutual exclusion rules, RimMind-Actions executes all actions regardless of RimChat status (does not affect Storyteller incidents)"));
            }

            SettingsUIDrawer.DrawSectionHeader(
                listing,
                RimMindAPI.Text.TrOr("RimMind.BridgeRimChat.Settings.Section.ContextPull", "Context Pull"));
            listing.CheckboxLabeled(
                RimMindAPI.Text.TrOr("RimMind.BridgeRimChat.Settings.EnableContextPull", "Enable context pull"),
                ref s.enableContextPull,
                RimMindAPI.Text.TrOr(
                    "RimMind.BridgeRimChat.Settings.EnableContextPull.Desc",
                    "Pull dialogue history and other data from RimChat, registering as RimMind context providers so RimMind can perceive RimChat's conversations"));
            if (s.enableContextPull)
            {
                listing.CheckboxLabeled(
                    "  " + RimMindAPI.Text.TrOr(
                        "RimMind.BridgeRimChat.Settings.PullDiplomacyHistory",
                        "Pull diplomacy dialogue history"),
                    ref s.pullDiplomacyHistory,
                    RimMindAPI.Text.TrOr(
                        "RimMind.BridgeRimChat.Settings.PullDiplomacyHistory.Desc",
                        "Register RimChat's diplomacy dialogue history as RimMind's static context provider"));
                listing.CheckboxLabeled(
                    "  " + RimMindAPI.Text.TrOr(
                        "RimMind.BridgeRimChat.Settings.PullRpgHistory",
                        "Pull RPG dialogue history"),
                    ref s.pullRpgHistory,
                    RimMindAPI.Text.TrOr(
                        "RimMind.BridgeRimChat.Settings.PullRpgHistory.Desc",
                        "Register RimChat's RPG dialogue history as RimMind's pawn context provider"));
            }
        }
    }
}
