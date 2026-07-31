namespace RimMind.Bridge.RimChat.Settings
{
    public partial class BridgeRimChatSettings
    {
        public bool enablePlayerInputGate = true;
        public bool enableChitchatGate = true;
        public bool enableAutoGate = true;
        public bool skipPlayerDialogue = true;
        public bool forceRimMindPlayerDialogue;

        public bool enableActionGate = true;
        public bool skipDiplomacyActions = true;
        public bool skipTriggerIncident = true;
        public bool skipSocialActions;
        public bool skipRecruitAgree;
        public int incidentCooldownTicks = 60000;
        public bool forceRimMindActions;

        public bool enableContextPull = true;
        public bool pullDiplomacyHistory = true;
        public bool pullRpgHistory;

        private static BridgeRimChatSettings? _instance;

        public BridgeRimChatSettings()
        {
            _instance = this;
        }

        public static BridgeRimChatSettings Get() =>
            _instance ?? new BridgeRimChatSettings();

        internal static void ResetForTesting() => _instance = null;

        public void ApplyDefaults()
        {
            enablePlayerInputGate = true;
            enableChitchatGate = true;
            enableAutoGate = true;
            skipPlayerDialogue = true;
            forceRimMindPlayerDialogue = false;

            enableActionGate = true;
            skipDiplomacyActions = true;
            skipTriggerIncident = true;
            skipSocialActions = false;
            skipRecruitAgree = false;
            incidentCooldownTicks = 60000;
            forceRimMindActions = false;

            enableContextPull = true;
            pullDiplomacyHistory = true;
            pullRpgHistory = false;
        }
    }
}
