using Verse;

namespace RimMind.Bridge.RimChat.Detection
{
    public static class RimChatDetector
    {
        public const string RimChatPackageId = "yancy.rimchat";

        public static bool IsRimChatActive => ModsConfig.IsActive(RimChatPackageId);
    }
}
