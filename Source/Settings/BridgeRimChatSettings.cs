using UnityEngine;
using Verse;
using RimMind.Core.UI;

namespace RimMind.Bridge.RimChat.Settings
{
    public class BridgeRimChatSettings : ModSettings
    {
        public bool enableDialogueGate = true;
        public bool skipPlayerDialogue = true;
        public bool forceRimMindPlayerDialogue = false;

        public bool enableActionGate = true;
        public bool skipDiplomacyActions = true;
        public bool skipTriggerIncident = true;
        public bool skipSocialActions = false;
        public bool skipRecruitAgree = false;
        public int incidentCooldownTicks = 60000;
        public bool forceRimMindActions = false;

        public bool enableContextExposure = true;
        public bool exposePersonality = true;
        public bool exposeMemory = true;
        public bool exposeStoryteller = false;
        public bool exposeAdvisorLog = false;

        private static BridgeRimChatSettings? _instance;
        public static BridgeRimChatSettings Get() => _instance ?? new BridgeRimChatSettings();

        public BridgeRimChatSettings()
        {
            _instance = this;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref enableDialogueGate, "enableDialogueGate", true);
            Scribe_Values.Look(ref skipPlayerDialogue, "skipPlayerDialogue", true);
            Scribe_Values.Look(ref forceRimMindPlayerDialogue, "forceRimMindPlayerDialogue", false);

            Scribe_Values.Look(ref enableActionGate, "enableActionGate", true);
            Scribe_Values.Look(ref skipDiplomacyActions, "skipDiplomacyActions", true);
            Scribe_Values.Look(ref skipTriggerIncident, "skipTriggerIncident", true);
            Scribe_Values.Look(ref skipSocialActions, "skipSocialActions", false);
            Scribe_Values.Look(ref skipRecruitAgree, "skipRecruitAgree", false);
            Scribe_Values.Look(ref incidentCooldownTicks, "incidentCooldownTicks", 60000);
            Scribe_Values.Look(ref forceRimMindActions, "forceRimMindActions", false);

            Scribe_Values.Look(ref enableContextExposure, "enableContextExposure", true);
            Scribe_Values.Look(ref exposePersonality, "exposePersonality", true);
            Scribe_Values.Look(ref exposeMemory, "exposeMemory", true);
            Scribe_Values.Look(ref exposeStoryteller, "exposeStoryteller", false);
            Scribe_Values.Look(ref exposeAdvisorLog, "exposeAdvisorLog", false);
        }

        private static Vector2 _scrollPos = Vector2.zero;

        public static void DrawSettingsContent(Rect inRect)
        {
            var s = Get();

            Rect contentArea = SettingsUIHelper.SplitContentArea(inRect);
            Rect bottomBar = SettingsUIHelper.SplitBottomBar(inRect);

            float contentH = EstimateHeight(s);
            Rect viewRect = new Rect(0f, 0f, contentArea.width - 16f, contentH);
            Widgets.BeginScrollView(contentArea, ref _scrollPos, viewRect);

            var listing = new Listing_Standard();
            listing.Begin(viewRect);

            SettingsUIHelper.DrawSectionHeader(listing, "RimMind.BridgeRimChat.Settings.Section.DialogueGate".Translate());
            listing.CheckboxLabeled("RimMind.BridgeRimChat.Settings.EnableDialogueGate".Translate(),
                ref s.enableDialogueGate,
                "RimMind.BridgeRimChat.Settings.EnableDialogueGate.Desc".Translate());
            if (s.enableDialogueGate)
            {
                listing.CheckboxLabeled("RimMind.BridgeRimChat.Settings.SkipPlayerDialogue".Translate(),
                    ref s.skipPlayerDialogue,
                    "RimMind.BridgeRimChat.Settings.SkipPlayerDialogue.Desc".Translate());
                if (s.skipPlayerDialogue)
                {
                    listing.CheckboxLabeled("  " + "RimMind.BridgeRimChat.Settings.ForceRimMindPlayerDialogue".Translate(),
                        ref s.forceRimMindPlayerDialogue,
                        "RimMind.BridgeRimChat.Settings.ForceRimMindPlayerDialogue.Desc".Translate());
                }
            }

            SettingsUIHelper.DrawSectionHeader(listing, "RimMind.BridgeRimChat.Settings.Section.ActionGate".Translate());
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
                    ref s.skipSocialActions);
                listing.CheckboxLabeled("  " + "RimMind.BridgeRimChat.Settings.SkipRecruitAgree".Translate(),
                    ref s.skipRecruitAgree);
                if (s.skipTriggerIncident)
                {
                    listing.Label("  " + "RimMind.BridgeRimChat.Settings.IncidentCooldown".Translate($"{s.incidentCooldownTicks / 60000f:F1}"));
                    s.incidentCooldownTicks = (int)listing.Slider(s.incidentCooldownTicks, 6000f, 180000f);
                    s.incidentCooldownTicks = (s.incidentCooldownTicks / 1500) * 1500;
                }
                listing.CheckboxLabeled("RimMind.BridgeRimChat.Settings.ForceRimMindActions".Translate(),
                    ref s.forceRimMindActions,
                    "RimMind.BridgeRimChat.Settings.ForceRimMindActions.Desc".Translate());
            }

            SettingsUIHelper.DrawSectionHeader(listing, "RimMind.BridgeRimChat.Settings.Section.ContextExposure".Translate());
            listing.CheckboxLabeled("RimMind.BridgeRimChat.Settings.EnableContextExposure".Translate(),
                ref s.enableContextExposure,
                "RimMind.BridgeRimChat.Settings.EnableContextExposure.Desc".Translate());
            if (s.enableContextExposure)
            {
                listing.CheckboxLabeled("  " + "RimMind.BridgeRimChat.Settings.ExposePersonality".Translate(),
                    ref s.exposePersonality);
                listing.CheckboxLabeled("  " + "RimMind.BridgeRimChat.Settings.ExposeMemory".Translate(),
                    ref s.exposeMemory);
                listing.CheckboxLabeled("  " + "RimMind.BridgeRimChat.Settings.ExposeStoryteller".Translate(),
                    ref s.exposeStoryteller);
                listing.CheckboxLabeled("  " + "RimMind.BridgeRimChat.Settings.ExposeAdvisorLog".Translate(),
                    ref s.exposeAdvisorLog);
            }

            listing.End();
            Widgets.EndScrollView();

            SettingsUIHelper.DrawBottomBar(bottomBar, () =>
            {
                s.enableDialogueGate = true;
                s.skipPlayerDialogue = true;
                s.forceRimMindPlayerDialogue = false;
                s.enableActionGate = true;
                s.skipDiplomacyActions = true;
                s.skipTriggerIncident = true;
                s.skipSocialActions = false;
                s.skipRecruitAgree = false;
                s.incidentCooldownTicks = 60000;
                s.forceRimMindActions = false;
                s.enableContextExposure = true;
                s.exposePersonality = true;
                s.exposeMemory = true;
                s.exposeStoryteller = false;
                s.exposeAdvisorLog = false;
            });

            Get().Write();
        }

        private static float EstimateHeight(BridgeRimChatSettings s)
        {
            float h = 30f;
            h += 24f + 24f;
            if (s.enableDialogueGate)
                h += 24f + (s.skipPlayerDialogue ? 24f : 0f);
            h += 24f + 24f;
            if (s.enableActionGate)
                h += 24f * 5 + (s.skipTriggerIncident ? 24f + 32f : 0f) + 24f;
            h += 24f + 24f;
            if (s.enableContextExposure)
                h += 24f * 4;
            return h + 40f;
        }
    }
}
