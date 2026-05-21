using RimMind.Application.Common.Interfaces.Extension;

namespace RimMind.Bridge.RimChat
{
    internal sealed class RimChatDialogueSkipCheck : ISkipCheck
    {
        private readonly RimMindBridgeRimChatMod _mod;
        public RimChatDialogueSkipCheck(RimMindBridgeRimChatMod mod) { _mod = mod; }
        public string Id => "rimchat_bridge_dialogue";
        public string OwnerModId => "RimMindBridgeRimChat";
        public SkipCheckKind Kind => SkipCheckKind.Dialogue;
        public bool ShouldSkip(in SkipCheckArgs args) => Bridge.DialogueGate.ShouldSkipDialogue((Verse.Pawn)args.Pawn, args.Trigger);
    }
}
