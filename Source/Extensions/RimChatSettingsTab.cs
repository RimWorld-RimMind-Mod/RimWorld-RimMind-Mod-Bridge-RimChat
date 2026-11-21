using RimMind.Bridge.RimChat.Settings;
using RimMind.Presentation.Api;
using RimMind.Presentation.Settings;
using UnityEngine;

namespace RimMind.Bridge.RimChat
{
    internal sealed class RimChatSettingsTab : ISettingsTab
    {
        public string Id => "bridge_rimchat";
        public string OwnerModId => "RimMindBridgeRimChat";
        public string Label => RimMindAPI.Text.TrOr("RimMind.BridgeRimChat.Settings.TabLabel", "RimChat Bridge");
        public void Draw(Rect rect) => BridgeRimChatSettings.DrawSettingsContent(rect);
    }
}
