using System;
using System.Reflection;
using HarmonyLib;
using RimMind.Domain.ValueObjects;
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

        internal static void ConfigureTypesForTesting(
            Type? apiType,
            Type? diplomacyManagerType,
            Type? rpgArchiveManagerType)
        {
            _apiType = apiType;
            _diplomacyManagerType = diplomacyManagerType;
            _rpgArchiveManagerType = rpgArchiveManagerType;
            _resolved = true;
        }

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
                RimMindErrors.Warn($"[RimMind-Bridge-RimChat] Failed to resolve RimChat types: {ex.Message}");
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
                RimMindErrors.Warn("[RimMind-Bridge-RimChat] GetStaticPropertyValue: type is null.");
                return null;
            }

            try
            {
                var prop = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Static);
                if (prop == null)
                {
                    RimMindErrors.Warn($"[RimMind-Bridge-RimChat] Static property '{propertyName}' not found on {type.Name}.");
                    return null;
                }
                return prop.GetValue(null);
            }
            catch (Exception ex)
            {
                RimMindErrors.Warn($"[RimMind-Bridge-RimChat] GetStaticPropertyValue({type.Name}.{propertyName}) failed: {ex.Message}");
                return null;
            }
        }

        public static object? GetInstanceFieldValue(object instance, string fieldName,
            BindingFlags flags = BindingFlags.Public | BindingFlags.Instance)
        {
            if (instance == null)
            {
                RimMindErrors.Warn("[RimMind-Bridge-RimChat] GetInstanceFieldValue: instance is null.");
                return null;
            }

            try
            {
                var field = instance.GetType().GetField(fieldName, flags);
                if (field == null)
                {
                    RimMindErrors.Warn($"[RimMind-Bridge-RimChat] Field '{fieldName}' not found on {instance.GetType().Name}.");
                    return null;
                }
                return field.GetValue(instance);
            }
            catch (Exception ex)
            {
                RimMindErrors.Warn($"[RimMind-Bridge-RimChat] GetInstanceFieldValue({instance.GetType().Name}.{fieldName}) failed: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 获取 RimChat manager 的单例实例（反射读取静态 "Instance" 属性）。
        /// 统一 BuildDiplomacyContext / BuildRpgContext 的 manager 获取逻辑。
        /// </summary>
        public static object? TryGetManagerInstance(Type? managerType, string instancePropertyName = "Instance")
        {
            if (managerType == null)
            {
                RimMindErrors.Warn($"[RimMind-Bridge-RimChat] TryGetManagerInstance: managerType is null.");
                return null;
            }

            var instance = GetStaticPropertyValue(managerType, instancePropertyName);
            if (instance == null)
            {
                RimMindErrors.Warn($"[RimMind-Bridge-RimChat] TryGetManagerInstance: instance is null on {managerType.Name}.");
                return null;
            }

            return instance;
        }
    }
}
