using System;
using System.Reflection;
using HarmonyLib;
using RimMind.Application.Features.Bridges;
using RimMind.Domain.ValueObjects;

namespace RimMind.Bridge.RimChat.Bridge
{
    public static class RimChatApiShim
    {
        private const string ApiTypeName = "RimChat.API.RimChatAPI";
        private const string DiplomacyManagerTypeName = "RimChat.DiplomacySystem.GameComponent_DiplomacyManager";
        private const string RpgArchiveManagerTypeName = "RimChat.Memory.RpgNpcDialogueArchiveManager";

        internal static readonly ReflectionAccessor Accessor = new ReflectionAccessor(
            typeName => AccessTools.TypeByName(typeName),
            msg => RimMindErrors.Warn(msg),
            "[RimMind-Bridge-RimChat]");

        private static Type? _apiType;
        private static Type? _diplomacyManagerType;
        private static Type? _rpgArchiveManagerType;
        private static bool _resolved;

        internal static void ConfigureTypesForTesting(
            Type? apiType,
            Type? diplomacyManagerType,
            Type? rpgArchiveManagerType)
        {
            Accessor.ResetForTesting();
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
                _apiType = Accessor.GetCachedType(ApiTypeName);
                _diplomacyManagerType = Accessor.GetCachedType(DiplomacyManagerTypeName);
                _rpgArchiveManagerType = Accessor.GetCachedType(RpgArchiveManagerTypeName);
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

        public static PropertyInfo? GetCachedProperty(
            Type type,
            string propertyName,
            BindingFlags flags = BindingFlags.Public | BindingFlags.Static)
            => Accessor.GetCachedProperty(type, propertyName, flags);

        public static FieldInfo? GetCachedField(
            Type type,
            string fieldName,
            BindingFlags flags = BindingFlags.Public | BindingFlags.Instance)
            => Accessor.GetCachedField(type, fieldName, flags);

        public static object? GetStaticPropertyValue(Type type, string propertyName)
            => Accessor.GetStaticPropertyValue(type, propertyName);

        public static object? GetInstanceFieldValue(
            object instance,
            string fieldName,
            BindingFlags flags = BindingFlags.Public | BindingFlags.Instance)
            => Accessor.GetInstanceFieldValue(instance, fieldName, flags);

        /// <summary>
        /// 获取 RimChat manager 的单例实例（反射读取静态 "Instance" 属性）。
        /// 统一 BuildDiplomacyContext / BuildRpgContext 的 manager 获取逻辑。
        /// </summary>
        public static object? TryGetManagerInstance(Type? managerType, string instancePropertyName = "Instance")
            => Accessor.TryGetSingletonInstance(managerType, instancePropertyName);
    }
}
