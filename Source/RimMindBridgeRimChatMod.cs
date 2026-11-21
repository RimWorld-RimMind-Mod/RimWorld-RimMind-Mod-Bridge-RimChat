using RimMind.Bridge.RimChat.Bridge;
using RimMind.Bridge.RimChat.Settings;
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

            RimMindAPI.Ext.Get<ISettingsTab>().Register(new RimChatSettingsTab());
            RimChatBridgeCoordinator.Register();
        }

        public override void DoSettingsWindowContents(UnityEngine.Rect rect)
        {
            BridgeRimChatSettings.DrawSettingsContent(rect);
        }
    }
}
