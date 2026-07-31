using RimMind.Bridge.RimChat.Bridge;
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

        public static void DrawSettingsContent(Rect inRect)
        {
            var s = Get();

            bool oldEnableContextPull = s.enableContextPull;
            bool oldPullDiplomacy = s.pullDiplomacyHistory;
            bool oldPullRpg = s.pullRpgHistory;

            Rect contentArea = SettingsUIDrawer.SplitContentArea(inRect);
            Rect bottomBar = SettingsUIDrawer.SplitBottomBar(inRect);

            float contentH = EstimateHeight(s);
            Rect viewRect = new Rect(0f, 0f, contentArea.width - 16f, contentH);
            Widgets.BeginScrollView(contentArea, ref _scrollPos, viewRect);

            var listing = new Listing_Standard();
            listing.Begin(viewRect);

            SettingsUIDrawer.DrawSectionHeader(listing, "RimMind.BridgeRimChat.Settings.Section.DialogueGate".Translate());
            listing.CheckboxLabeled("RimMind.BridgeRimChat.Settings.EnablePlayerInputGate".Translate(),
                ref s.enablePlayerInputGate,
                "RimMind.BridgeRimChat.Settings.EnablePlayerInputGate.Desc".Translate());
            if (s.enablePlayerInputGate)
            {
                listing.CheckboxLabeled("  " + "RimMind.BridgeRimChat.Settings.SkipPlayerDialogue".Translate(),
                    ref s.skipPlayerDialogue,
                    "RimMind.BridgeRimChat.Settings.SkipPlayerDialogue.Desc".Translate());
                if (s.skipPlayerDialogue)
                {
                    listing.CheckboxLabeled("    " + "RimMind.BridgeRimChat.Settings.ForceRimMindPlayerDialogue".Translate(),
                        ref s.forceRimMindPlayerDialogue,
                        "RimMind.BridgeRimChat.Settings.ForceRimMindPlayerDialogue.Desc".Translate());
                }
            }
            listing.CheckboxLabeled("RimMind.BridgeRimChat.Settings.EnableChitchatGate".Translate(),
                ref s.enableChitchatGate,
                "RimMind.BridgeRimChat.Settings.EnableChitchatGate.Desc".Translate());
            listing.CheckboxLabeled("RimMind.BridgeRimChat.Settings.EnableAutoGate".Translate(),
                ref s.enableAutoGate,
                "RimMind.BridgeRimChat.Settings.EnableAutoGate.Desc".Translate());

            SettingsUIDrawer.DrawSectionHeader(listing, "RimMind.BridgeRimChat.Settings.Section.ActionGate".Translate());
            listing.CheckboxLabeled("RimMind.BridgeRimChat.Settings.EnableActionGate".Translate(),
                ref s.enableActionGate,
                "RimMind.BridgeRimChat.Settings.EnableActionGate.Desc".Translate());
            if (s.enableActionGate)
            {
                listing.CheckboxLabeled("  " + "RimMind.BridgeRimChat.Settings.SkipDiplomacyActions".Translate(),
                    ref s.skipDiplomacyActions,
                    "RimMind.BridgeRimChat.Settings.SkipDiplomacyActions.Desc".Translate());
                listing.CheckboxLabeled("  " + "RimMind.BridgeRimChat.Settings.SkipTriggerIncident".Translate(),
                    ref s.skipTriggerIncident,
                    "RimMind.BridgeRimChat.Settings.SkipTriggerIncident.Desc".Translate());
                listing.CheckboxLabeled("  " + "RimMind.BridgeRimChat.Settings.SkipSocialActions".Translate(),
                    ref s.skipSocialActions,
                    "RimMind.BridgeRimChat.Settings.SkipSocialActions.Desc".Translate());
                listing.CheckboxLabeled("  " + "RimMind.BridgeRimChat.Settings.SkipRecruitAgree".Translate(),
                    ref s.skipRecruitAgree,
                    "RimMind.BridgeRimChat.Settings.SkipRecruitAgree.Desc".Translate());
                if (s.skipTriggerIncident)
                {
                    listing.Label("  " + "RimMind.BridgeRimChat.Settings.IncidentCooldown".Translate($"{s.incidentCooldownTicks / 60000f:F1}"));
                    GUI.color = Color.gray;
                    listing.Label("    " + "RimMind.BridgeRimChat.Settings.IncidentCooldown.Desc".Translate());
                    GUI.color = Color.white;
                    s.incidentCooldownTicks = (int)listing.Slider(s.incidentCooldownTicks, 6000f, 180000f);
                    s.incidentCooldownTicks = (s.incidentCooldownTicks / 1500) * 1500;
                }
                listing.CheckboxLabeled("RimMind.BridgeRimChat.Settings.ForceRimMindActions".Translate(),
                    ref s.forceRimMindActions,
                    "RimMind.BridgeRimChat.Settings.ForceRimMindActions.Desc".Translate());
            }

            SettingsUIDrawer.DrawSectionHeader(listing, "RimMind.BridgeRimChat.Settings.Section.ContextPull".Translate());
            listing.CheckboxLabeled("RimMind.BridgeRimChat.Settings.EnableContextPull".Translate(),
                ref s.enableContextPull,
                "RimMind.BridgeRimChat.Settings.EnableContextPull.Desc".Translate());
            if (s.enableContextPull)
            {
                listing.CheckboxLabeled("  " + "RimMind.BridgeRimChat.Settings.PullDiplomacyHistory".Translate(),
                    ref s.pullDiplomacyHistory,
                    "RimMind.BridgeRimChat.Settings.PullDiplomacyHistory.Desc".Translate());
                listing.CheckboxLabeled("  " + "RimMind.BridgeRimChat.Settings.PullRpgHistory".Translate(),
                    ref s.pullRpgHistory,
                    "RimMind.BridgeRimChat.Settings.PullRpgHistory.Desc".Translate());
            }

            listing.End();
            Widgets.EndScrollView();

            SettingsUIDrawer.DrawBottomBar(bottomBar, () =>
            {
                s.ApplyDefaults();
            });

            if (s.enableContextPull != oldEnableContextPull
                || s.pullDiplomacyHistory != oldPullDiplomacy
                || s.pullRpgHistory != oldPullRpg)
            {
                ContextPullBridge.Refresh();
            }

            Get().Write();
        }

        private static float EstimateHeight(BridgeRimChatSettings s)
        {
            float h = 30f;
            h += 24f + 24f;
            if (s.enablePlayerInputGate)
                h += 24f + (s.skipPlayerDialogue ? 24f : 0f);
            h += 24f + 24f;
            h += 24f + 24f;
            if (s.enableActionGate)
                h += 24f * 4 + (s.skipTriggerIncident ? 24f + 24f + 32f : 0f) + 24f;
            h += 24f + 24f;
            if (s.enableContextPull)
                h += 24f * 2;
            return h + 40f;
        }
    }
}
