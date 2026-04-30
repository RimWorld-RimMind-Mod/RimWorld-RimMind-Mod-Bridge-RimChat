using System;
using System.Reflection;
using HarmonyLib;
using RimMind.Bridge.RimChat.Detection;
using Verse;

namespace RimMind.Bridge.RimChat.Bridge
{
    public static class RimChatApiShim
    {
        private const string ApiTypeName = "RimChat.API.RimChatAPI";
        private const string DiplomacyManagerTypeName = "RimChat.DiplomacySystem.GameComponent_DiplomacyManager";
        private const string RpgArchiveManagerTypeName = "RimChat.Memory.RpgNpcDialogueArchiveManager";

        private static Type? _apiType;
        private static Type? _diplomacyManagerType;
        private static Type? _rpgArchiveManagerType;
        private static bool _resolved;

        public static bool IsAvailable => RimChatDetector.IsRimChatApiAvailable;

        private static void EnsureResolved()
        {
            if (_resolved) return;
            _resolved = true;

            ResolveTypes();
        }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        private static void ResolveTypes()
        {
            try
            {
                _apiType = AccessTools.TypeByName(ApiTypeName);
                _diplomacyManagerType = AccessTools.TypeByName(DiplomacyManagerTypeName);
                _rpgArchiveManagerType = AccessTools.TypeByName(RpgArchiveManagerTypeName);
            }
            catch (Exception ex)
            {
                Log.Warning($"[RimMind-Bridge-RimChat] Failed to resolve RimChat types: {ex.Message}");
            }
        }

        public static Type? ApiType
        {
            get { EnsureResolved(); return _apiType; }
        }

        public static Type? DiplomacyManagerType
        {
            get { EnsureResolved(); return _diplomacyManagerType; }
        }

        public static Type? RpgArchiveManagerType
        {
            get { EnsureResolved(); return _rpgArchiveManagerType; }
        }

        public static object? GetStaticPropertyValue(Type type, string propertyName)
        {
            if (type == null)
            {
                Log.Warning("[RimMind-Bridge-RimChat] GetStaticPropertyValue: type is null.");
                return null;
            }

            try
            {
                var prop = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Static);
                if (prop == null)
                {
                    Log.Warning($"[RimMind-Bridge-RimChat] Static property '{propertyName}' not found on {type.Name}.");
                    return null;
                }
                return prop.GetValue(null);
            }
            catch (Exception ex)
            {
                Log.Warning($"[RimMind-Bridge-RimChat] GetStaticPropertyValue({type.Name}.{propertyName}) failed: {ex.Message}");
                return null;
            }
        }

        public static object? GetInstanceFieldValue(object instance, string fieldName,
            BindingFlags flags = BindingFlags.Public | BindingFlags.Instance)
        {
            try
            {
                var field = instance.GetType().GetField(fieldName, flags);
                if (field == null)
                {
                    Log.Warning($"[RimMind-Bridge-RimChat] Field '{fieldName}' not found on {instance.GetType().Name}.");
                    return null;
                }
                return field.GetValue(instance);
            }
            catch (Exception ex)
            {
                Log.Warning($"[RimMind-Bridge-RimChat] GetInstanceFieldValue({instance.GetType().Name}.{fieldName}) failed: {ex.Message}");
                return null;
            }
        }
    }
}
