using RimMind.Application.Common.Interfaces.Extension;

namespace RimMind.Bridge.RimChat
{
    internal sealed class RimChatIncidentExecutedListener : IIncidentExecutedListener
    {
        public string Id => "rimchat_bridge_incident";
        public string OwnerModId => "RimMind.BridgeRimChat";
        public void OnIncidentExecuted() => Cooldown.SharedIncidentCooldown.RecordIncident();
    }
}
