using HarmonyLib;
using Verse;

namespace RimMind.Bridge.RimChat.Detection
{
    public static class RimChatDetector
    {
        public const string RimChatPackageId = "yancy.rimchat";

        public static bool IsRimChatActive => ModsConfig.IsActive(RimChatPackageId);

        private static bool? _apiAvailable;
        private static bool _apiChecked;

        public static bool IsRimChatApiAvailable
        {
            get
            {
                if (!_apiChecked)
                {
                    _apiAvailable = IsRimChatActive && AccessTools.TypeByName("RimChat.API.RimChatAPI") != null;
                    _apiChecked = true;
                }
                return _apiAvailable ?? false;
            }
        }
    }
}
