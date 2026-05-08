using RimMind.Bridge.RimChat.Bridge;
using RimMind.Bridge.RimChat.Detection;
using RimMind.Bridge.RimChat.Settings;
using RimMind.Contracts.Extension;
using RimMind.Core;
using Verse;

namespace RimMind.Bridge.RimChat
{
    public class RimMindBridgeRimChatMod : Mod
    {
        public RimMindBridgeRimChatMod(ModContentPack content) : base(content)
        {
            GetSettings<BridgeRimChatSettings>();

            RimMindAPI.Extensions<ISettingsTab>().Register(new RimChatSettingsTab());

            if (!RimChatDetector.IsRimChatActive)
            {
                Log.Message("[RimMind-Bridge-RimChat] RimChat not active, bridge modules skipped.");
                return;
            }

            RimMindAPI.Extensions<ISkipCheck>().Register(new RimChatDialogueSkipCheck(this));
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

        public override string SettingsCategory() => "RimMind.BridgeRimChat.Settings.Category".Translate();
    }
}
