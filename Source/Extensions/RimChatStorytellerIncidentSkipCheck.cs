using RimMind.Application.Common.Interfaces.Extension;

namespace RimMind.Bridge.RimChat
{
    internal sealed class RimChatStorytellerIncidentSkipCheck : ISkipCheck
    {
        public string Id => "rimchat_bridge_storyteller";
        public string OwnerModId => "RimMindBridgeRimChat";
        public SkipCheckKind Kind => SkipCheckKind.StorytellerIncident;
        public bool ShouldSkip(in SkipCheckArgs args) => Bridge.ActionGate.ShouldSkipStorytellerIncident();
    }
}
