using RimMind.Application.Common.Interfaces.Extension;

namespace RimMind.Bridge.RimChat
{
    internal sealed class RimChatActionSkipCheck : ISkipCheck
    {
        public string Id => "rimchat_bridge_action";
        public SkipCheckKind Kind => SkipCheckKind.Action;
        public bool ShouldSkip(in SkipCheckArgs args) => Bridge.ActionGate.ShouldSkipAction(args.IntentId);
    }
}
