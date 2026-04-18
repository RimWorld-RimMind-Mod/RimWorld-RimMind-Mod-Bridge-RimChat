using HarmonyLib;
using RimMind.Bridge.RimChat.Bridge;
using RimMind.Bridge.RimChat.Cooldown;
using RimMind.Bridge.RimChat.Detection;
using RimMind.Bridge.RimChat.Settings;
using RimMind.Core;
using Verse;

namespace RimMind.Bridge.RimChat
{
    public class RimMindBridgeRimChatMod : Mod
    {
        public static BridgeRimChatSettings Settings = null!;

        public RimMindBridgeRimChatMod(ModContentPack content) : base(content)
        {
            Settings = GetSettings<BridgeRimChatSettings>();
            new Harmony("mcocdaa.RimMindBridgeRimChat").PatchAll();

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

            ContextExposureBridge.Register();
            Log.Message("[RimMind-Bridge-RimChat] ContextExposure registered.");

            Log.Message("[RimMind-Bridge-RimChat] Initialized.");
        }

        public override string SettingsCategory() => "RimMind - Bridge (RimChat)";

        public override void DoSettingsWindowContents(UnityEngine.Rect rect)
        {
            BridgeRimChatSettings.DrawSettingsContent(rect);
        }
    }
}
