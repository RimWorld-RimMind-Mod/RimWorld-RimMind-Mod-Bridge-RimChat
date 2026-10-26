using System;
using System.Collections.Concurrent;
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

        private static readonly ConcurrentDictionary<(Type Type, string Name, BindingFlags Flags), PropertyInfo?> _propertyCache =
            new ConcurrentDictionary<(Type Type, string Name, BindingFlags Flags), PropertyInfo?>();

        private static readonly ConcurrentDictionary<(Type Type, string Name, BindingFlags Flags), FieldInfo?> _fieldCache =
            new ConcurrentDictionary<(Type Type, string Name, BindingFlags Flags), FieldInfo?>();

        private static readonly ConcurrentDictionary<string, byte> _warnedMissingMembers =
            new ConcurrentDictionary<string, byte>(StringComparer.Ordinal);

        internal static void ConfigureTypesForTesting(
            Type? apiType,
            Type? diplomacyManagerType,
            Type? rpgArchiveManagerType)
        {
            _apiType = apiType;
            _diplomacyManagerType = diplomacyManagerType;
            _rpgArchiveManagerType = rpgArchiveManagerType;
            _resolved = true;
            _propertyCache.Clear();
            _fieldCache.Clear();
            _warnedMissingMembers.Clear();
        }

        private static void WarnOnce(string key, string message)
        {
            if (_warnedMissingMembers.TryAdd(key, 1))
            {
                RimMindErrors.Warn(message);
            }
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

        public static PropertyInfo? GetCachedProperty(Type type, string propertyName,
            BindingFlags flags = BindingFlags.Public | BindingFlags.Static)
        {
            if (type == null || string.IsNullOrEmpty(propertyName)) return null;
            var prop = _propertyCache.GetOrAdd(
                (type, propertyName, flags),
                static k => k.Type.GetProperty(k.Name, k.Flags));
            if (prop == null)
            {
                WarnOnce(
                    $"prop:{type.FullName}:{propertyName}:{flags}",
                    $"[RimMind-Bridge-RimChat] Property '{propertyName}' not found on {type.Name}.");
            }
            return prop;
        }

        public static FieldInfo? GetCachedField(Type type, string fieldName,
            BindingFlags flags = BindingFlags.Public | BindingFlags.Instance)
        {
            if (type == null || string.IsNullOrEmpty(fieldName)) return null;
            var field = _fieldCache.GetOrAdd(
                (type, fieldName, flags),
                static k => k.Type.GetField(k.Name, k.Flags));
            if (field == null)
            {
                WarnOnce(
                    $"field:{type.FullName}:{fieldName}:{flags}",
                    $"[RimMind-Bridge-RimChat] Field '{fieldName}' not found on {type.Name}.");
            }
            return field;
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
                var prop = GetCachedProperty(type, propertyName, BindingFlags.Public | BindingFlags.Static);
                if (prop == null) return null;
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
                var field = GetCachedField(instance.GetType(), fieldName, flags);
                if (field == null) return null;
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
