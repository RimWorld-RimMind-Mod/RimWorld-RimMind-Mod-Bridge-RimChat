using System;
using LudeonTK;
using RimMind.Bridge.RimChat.Bridge;
using RimMind.Bridge.RimChat.Cooldown;
using RimMind.Bridge.RimChat.Detection;
using RimMind.Bridge.RimChat.Settings;
using RimMind.Presentation.Api;
using RimWorld;
using Verse;

namespace RimMind.Bridge.RimChat.Debug
{
    /// <summary>
    /// In-game behavioral autotest suite for RimMind-Bridge-RimChat.
    /// Discovered automatically by Core's BehaviorAutotestRunner and exposed to Dev menu.
    /// </summary>
    public sealed class BridgeRimChatBehaviorAutotestSuite : BehaviorAutotestSuiteBase
    {
        public override string ModId => "BridgeRimChat";
        public override string SuiteId => "Behavior.BridgeRimChat";

        [DebugAction("Autotests", "Run Bridge-RimChat In-Game Behavior Test", actionType = DebugActionType.Action)]
        public static void RunFromDevMenu() => RunSuiteFromDevMenu<BridgeRimChatBehaviorAutotestSuite>();

        public override void RunSuite(IInGameBehaviorSuiteContext context)
        {
            // 1. Detection Safety Check
            try
            {
                bool isActive = RimChatDetector.IsRimChatActive;
                context.Assert(true, $"RimChatDetector executed safely (Active: {isActive})");
            }
            catch (Exception ex)
            {
                context.Assert(false, $"RimChatDetector threw exception: {ex.Message}");
            }

            // 2. ActionGate Logic Verification
            try
            {
                bool skipDiplomacy = ActionGate.ShouldSkipAction("adjust_faction");
                bool skipSocial = ActionGate.ShouldSkipAction("romance_attempt");
                bool skipRecruit = ActionGate.ShouldSkipAction("recruit_agree");

                if (!RimChatDetector.IsRimChatActive)
                {
                    context.Assert(!skipDiplomacy && !skipSocial && !skipRecruit, "ActionGate passes through all actions when RimChat is inactive");
                }
                else
                {
                    context.Assert(true, $"ActionGate evaluated with RimChat active (skipDiplomacy={skipDiplomacy})");
                }
            }
            catch (Exception ex)
            {
                context.Assert(false, $"ActionGate evaluation threw exception: {ex.Message}");
            }

            // 3. DialogueGate Logic Verification
            Pawn? pawn = context.ActiveColonist;
            try
            {
                bool skipChitchat = DialogueGate.ShouldSkipDialogue(pawn, "Chitchat");
                bool skipAuto = DialogueGate.ShouldSkipDialogue(pawn, "Auto");

                if (!RimChatDetector.IsRimChatActive)
                {
                    context.Assert(!skipChitchat && !skipAuto, "DialogueGate passes through all dialogue events when RimChat is inactive");
                }
                else
                {
                    context.Assert(true, $"DialogueGate evaluated with RimChat active (skipChitchat={skipChitchat})");
                }
            }
            catch (Exception ex)
            {
                context.Assert(false, $"DialogueGate evaluation threw exception: {ex.Message}");
            }

            // 4. SharedIncidentCooldown Safety
            try
            {
                bool onCd = SharedIncidentCooldown.IsOnCooldown(60000);
                context.Assert(true, $"SharedIncidentCooldown query executed safely (onCooldown: {onCd})");
            }
            catch (Exception ex)
            {
                context.Assert(false, $"SharedIncidentCooldown threw exception: {ex.Message}");
            }

            // 5. Settings Accessibility
            var settings = BridgeRimChatSettings.Get();
            context.Assert(settings != null, "BridgeRimChatSettings accessible and loaded");
        }
    }
}
