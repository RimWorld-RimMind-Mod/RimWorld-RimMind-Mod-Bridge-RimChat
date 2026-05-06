using RimMind.Contracts.Extension;

namespace RimMind.Bridge.RimChat
{
    internal sealed class RimChatFloatMenuSkipCheck : ISkipCheck
    {
        public string Id => "rimchat_bridge_floatmenu";
        public SkipCheckKind Kind => SkipCheckKind.FloatMenu;
        public bool ShouldSkip(in SkipCheckArgs args) => Bridge.DialogueGate.ShouldSkipFloatMenuOption();
    }
}
