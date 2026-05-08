using RimMind.Contracts.Extension;
using RimMind.Bridge.RimChat.Settings;
using UnityEngine;
using Verse;

namespace RimMind.Bridge.RimChat
{
    internal sealed class RimChatSettingsTab : ISettingsTab
    {
        public string Id => "bridge_rimchat";
        public string Label => "RimMind.BridgeRimChat.Settings.TabLabel".Translate();
        public void Draw(Rect rect) => BridgeRimChatSettings.DrawSettingsContent(rect);
    }
}
