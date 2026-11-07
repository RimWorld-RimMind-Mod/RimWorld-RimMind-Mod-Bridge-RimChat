using RimMind.Application.Common.Interfaces.Extension;

namespace RimMind.Bridge.RimChat
{
    internal sealed class RimChatActionSkipCheck : IChannelSkipCheck
    {
        public string Id => "rimchat_bridge_action";
        public string OwnerModId => "RimMindBridgeRimChat";
        public SkipCheckKind Kind => SkipCheckKind.Action;
        public string Channel => SkipCheckChannels.Action;
        public bool ShouldSkip(in SkipCheckArgs args) => Bridge.ActionGate.ShouldSkipAction(args.IntentId ?? args.Tag);
    }
}
