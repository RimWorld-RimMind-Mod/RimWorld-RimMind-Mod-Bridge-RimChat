using RimMind.Bridge.RimChat.Bridge;
using RimMind.Bridge.RimChat.Detection;
using RimMind.Bridge.RimChat.Settings;
using RimMind.Application.Common.Interfaces.Extension;
using RimMind.Presentation;
using RimMind.Presentation.Api;
using RimMind.Presentation.Settings;
using Verse;

namespace RimMind.Bridge.RimChat
{
    public class RimMindBridgeRimChatMod : RimMindSubmodBase<BridgeRimChatSettings>
    {
        public static new BridgeRimChatSettings Settings = null!;

        public RimMindBridgeRimChatMod(ModContentPack content) : base(content)
        {
            Settings = base.Settings;

            RimMindAPI.Extensions<ISettingsTab>().Register(new RimChatSettingsTab());

            if (!RimChatDetector.IsRimChatActive)
            {
                Log.Message("[RimMind-Bridge-RimChat] RimChat not active, bridge modules skipped.");
                return;
            }

            RimMindAPI.Extensions<ISkipCheck>().Register(new RimChatDialogueSkipCheck());
            RimMindAPI.Extensions<ISkipCheck>().Register(new RimChatFloatMenuSkipCheck());
            Log.Message("[RimMind-Bridge-RimChat] DialogueGate registered.");

            RimMindAPI.Extensions<ISkipCheck>().Register(new RimChatActionSkipCheck());
            RimMindAPI.Extensions<ISkipCheck>().Register(new RimChatStorytellerIncidentSkipCheck());
            RimMindAPI.Extensions<IIncidentExecutedListener>().Register(new RimChatIncidentExecutedListener());
            Log.Message("[RimMind-Bridge-RimChat] ActionGate registered.");

            ContextPullBridge.Register();
            Log.Message("[RimMind-Bridge-RimChat] ContextPull registered.");

            Log.Message("[RimMind-Bridge-RimChat] Initialized.");
        }

        public override void DoSettingsWindowContents(UnityEngine.Rect rect)
        {
            BridgeRimChatSettings.DrawSettingsContent(rect);
        }
    }
}
