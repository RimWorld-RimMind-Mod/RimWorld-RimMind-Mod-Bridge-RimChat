using RimMind.Application.Common.Interfaces.Extension;

namespace RimMind.Bridge.RimChat
{
    internal sealed class RimChatFloatMenuSkipCheck : ISkipCheck
    {
        public string Id => "rimchat_bridge_floatmenu";
        public string OwnerModId => "RimMindBridgeRimChat";
        public SkipCheckKind Kind => SkipCheckKind.FloatMenu;
        public bool ShouldSkip(in SkipCheckArgs args) => Bridge.DialogueGate.ShouldSkipFloatMenuOption();
    }
}
