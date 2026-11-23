using System;
using System.Reflection;
using RimMind.Application.Features.Bridges;
using RimMind.Presentation.Api;

namespace RimMind.Bridge.RimChat.Bridge
{
    public static class RimChatApiShim
    {
        private const string DiplomacyManagerTypeName = "RimChat.DiplomacySystem.GameComponent_DiplomacyManager";
        private const string RpgArchiveManagerTypeName = "RimChat.Memory.RpgNpcDialogueArchiveManager";

        internal static readonly ReflectionAccessor Accessor =
            RimMindAPI.Bridges.CreateReflectionAccessor("[RimMind-Bridge-RimChat]");

        internal static void ConfigureTypesForTesting(
            Type? diplomacyManagerType,
            Type? rpgArchiveManagerType)
        {
            Accessor.ResetForTesting();
            Accessor.SetCachedType(DiplomacyManagerTypeName, diplomacyManagerType);
            Accessor.SetCachedType(RpgArchiveManagerTypeName, rpgArchiveManagerType);
        }

        public static Type? DiplomacyManagerType => Accessor.GetCachedType(DiplomacyManagerTypeName);

        public static Type? RpgArchiveManagerType => Accessor.GetCachedType(RpgArchiveManagerTypeName);

        public static FieldInfo? GetCachedField(
            Type type,
            string fieldName,
            BindingFlags flags = BindingFlags.Public | BindingFlags.Instance)
            => Accessor.GetCachedField(type, fieldName, flags);

        public static MethodInfo? GetCachedMethod(
            Type type,
            string methodName,
            BindingFlags flags = BindingFlags.Public | BindingFlags.Static,
            Type[]? parameterTypes = null,
            bool warnIfMissing = true)
            => Accessor.GetCachedMethod(type, methodName, flags, parameterTypes, warnIfMissing);

        public static object? GetStaticPropertyValue(Type type, string propertyName)
            => Accessor.GetStaticPropertyValue(type, propertyName);

        public static object? GetInstanceFieldValue(
            object instance,
            string fieldName,
            BindingFlags flags = BindingFlags.Public | BindingFlags.Instance)
            => Accessor.GetInstanceFieldValue(instance, fieldName, flags);

        /// <summary>
        /// Resolves a RimChat manager singleton instance from either a static property or static field ("Instance"),
        /// deduplicating missing-member and runtime warnings via Core's ReflectionAccessor.
        /// </summary>
        public static object? TryGetManagerInstance(Type? managerType, string instanceMemberName = "Instance")
            => Accessor.TryGetSingletonInstance(managerType, instanceMemberName);
    }
}
