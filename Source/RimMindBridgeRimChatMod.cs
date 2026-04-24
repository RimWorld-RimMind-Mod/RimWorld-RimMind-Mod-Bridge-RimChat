using RimMind.Bridge.RimChat.Bridge;
using RimMind.Bridge.RimChat.Detection;
using RimMind.Bridge.RimChat.Settings;
using RimMind.Core;
using Verse;

namespace RimMind.Bridge.RimChat
{
    public class RimMindBridgeRimChatMod : Mod
    {
        public RimMindBridgeRimChatMod(ModContentPack content) : base(content)
        {
            GetSettings<BridgeRimChatSettings>();

            RimMindAPI.RegisterSettingsTab("bridge_rimchat",
                () => "RimMind.BridgeRimChat.Settings.TabLabel".Translate(),
                BridgeRimChatSettings.DrawSettingsContent);

            if (!RimChatDetector.IsRimChatActive)
            {
                Log.Message("[RimMind-Bridge-RimChat] RimChat not active, bridge modules skipped.");
                return;
            }

            DialogueGate.RegisterSkipChecks();
            Log.Message("[RimMind-Bridge-RimChat] DialogueGate registered.");

            ActionGate.Register();
            Log.Message("[RimMind-Bridge-RimChat] ActionGate registered.");

            ContextPullBridge.Register();
            Log.Message("[RimMind-Bridge-RimChat] ContextPull registered.");

            Log.Message("[RimMind-Bridge-RimChat] Initialized.");
        }

        public override string SettingsCategory() => "RimMind.BridgeRimChat.Settings.Category".Translate();
    }
}
