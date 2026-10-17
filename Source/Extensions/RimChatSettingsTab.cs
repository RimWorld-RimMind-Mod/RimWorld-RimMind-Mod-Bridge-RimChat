using RimMind.Presentation.Settings;
using RimMind.Bridge.RimChat.Settings;
using UnityEngine;
using Verse;

namespace RimMind.Bridge.RimChat
{
    internal sealed class RimChatSettingsTab : ISettingsTab
    {
        public string Id => "bridge_rimchat";
        public string OwnerModId => "RimMindBridgeRimChat";
        public string Label => "RimMind.BridgeRimChat.Settings.TabLabel".CanTranslate()
            ? (string)"RimMind.BridgeRimChat.Settings.TabLabel".Translate()
            : "RimChat Bridge";
        public void Draw(Rect rect) => BridgeRimChatSettings.DrawSettingsContent(rect);
    }
}
