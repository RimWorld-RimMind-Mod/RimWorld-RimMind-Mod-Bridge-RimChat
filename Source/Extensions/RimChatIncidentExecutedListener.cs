using RimMind.Application.Common.Interfaces.Extension;

namespace RimMind.Bridge.RimChat
{
    internal sealed class RimChatIncidentExecutedListener : IIncidentExecutedListener
    {
        public string Id => "rimchat_bridge_incident";
        public string OwnerModId => "RimMindBridgeRimChat";
        public void OnIncidentExecuted()
        {
            if (!Detection.RimChatDetector.IsRimChatActive) return;
            var settings = Settings.BridgeRimChatSettings.Get();
            if (!settings.enableActionGate || !settings.skipTriggerIncident) return;
            Cooldown.SharedIncidentCooldown.RecordIncident();
        }
    }
}
