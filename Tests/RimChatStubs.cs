namespace HarmonyLib
{
    public static class AccessTools
    {
        public static System.Type? TypeByName(string name) => null;
    }
}

namespace Verse
{
    public static class Log
    {
        public static void Warning(string msg) { }
        public static void Message(string msg) { }
        public static void Error(string msg) { }
    }

    public class Pawn { }

    public static class Find
    {
        public static TickManager TickManager = new TickManager();
    }

    public class TickManager
    {
        public int TicksGame = 100000;
    }

    public static class Scribe_Values
    {
        public static void Look<T>(ref T value, string label, T defaultValue = default) { }
    }

    public class ModSettings
    {
        public virtual void ExposeData() { }
    }
}

namespace RimMind.Bridge.RimChat.Detection
{
    public static class RimChatDetector
    {
        public static bool IsRimChatApiAvailable { get; set; }
        public static bool IsRimChatActive { get; set; }
    }
}

namespace RimMind.Bridge.RimChat.Settings
{
    public class BridgeRimChatSettings : Verse.ModSettings
    {
        public bool enablePlayerInputGate = true;
        public bool enableChitchatGate = true;
        public bool enableAutoGate = true;
        public bool skipPlayerDialogue = true;
        public bool forceRimMindPlayerDialogue = false;

        public bool enableActionGate = true;
        public bool skipDiplomacyActions = true;
        public bool skipTriggerIncident = true;
        public bool skipSocialActions = false;
        public bool skipRecruitAgree = false;
        public int incidentCooldownTicks = 60000;
        public bool forceRimMindActions = false;

        public bool enableContextPull = true;
        public bool pullDiplomacyHistory = true;
        public bool pullRpgHistory = false;

        private static BridgeRimChatSettings? _instance;
        public static BridgeRimChatSettings Get() => _instance ??= new BridgeRimChatSettings();

        public BridgeRimChatSettings() { _instance = this; }

        public static void Reset() { _instance = null; }
    }
}

namespace RimMind.Core
{
    public static class RimMindAPI
    {
        public static int DialogueSkipCheckCount { get; set; }
        public static int FloatMenuSkipCheckCount { get; set; }
        public static int ActionSkipCheckCount { get; set; }
        public static int IncidentCallbackCount { get; set; }
        public static int StorytellerSkipCheckCount { get; set; }

        public static void RegisterDialogueSkipCheck(string sourceId, System.Func<Verse.Pawn, string, bool> check)
        {
            DialogueSkipCheckCount++;
        }

        public static void RegisterFloatMenuSkipCheck(string sourceId, System.Func<bool> check)
        {
            FloatMenuSkipCheckCount++;
        }

        public static void RegisterActionSkipCheck(string sourceId, System.Func<string, bool> check)
        {
            ActionSkipCheckCount++;
        }

        public static string RegisterIncidentExecutedCallback(System.Action callback)
        {
            IncidentCallbackCount++;
            return $"cb_{IncidentCallbackCount}";
        }

        public static void UnregisterIncidentExecutedCallback(string key)
        {
            IncidentCallbackCount--;
        }

        public static string RegisterStorytellerIncidentSkipCheck(System.Func<bool> check)
        {
            StorytellerSkipCheckCount++;
            return $"sc_{StorytellerSkipCheckCount}";
        }

        public static void UnregisterStorytellerIncidentSkipCheck(string key)
        {
            StorytellerSkipCheckCount--;
        }

        public static void ResetCounts()
        {
            DialogueSkipCheckCount = 0;
            FloatMenuSkipCheckCount = 0;
            ActionSkipCheckCount = 0;
            IncidentCallbackCount = 0;
            StorytellerSkipCheckCount = 0;
        }
    }
}
