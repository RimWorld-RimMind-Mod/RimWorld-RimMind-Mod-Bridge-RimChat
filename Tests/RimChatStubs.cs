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
}

namespace RimMind.Bridge.RimChat.Detection
{
    public static class RimChatDetector
    {
        public static bool IsRimChatApiAvailable { get; set; }
        public static bool IsRimChatActive { get; set; }
    }
}
