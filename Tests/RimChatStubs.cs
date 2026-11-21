using System.Linq;
using System.Collections.Generic;

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

    public class Pawn
    {
        public int thingIDNumber;
        public bool Dead;
        public bool Destroyed;
        public bool Spawned = true;
        public bool IsPrisoner;
    }

    public static class Find
    {
        public static TickManager TickManager = new TickManager();
        public static WorldPawnsHolder WorldPawns = new WorldPawnsHolder();
        public static MapHolder? CurrentMap = null;
        /// <summary>
        /// 所有已加载地图。测试环境默认空列表，源码中 TryFindPawnById 会遍历此列表。
        /// </summary>
        public static List<MapHolder> Maps = new List<MapHolder>();
    }

    public class TickManager
    {
        public int TicksGame = 100000;
    }

    public class WorldPawnsHolder
    {
        public List<Pawn> AllPawnsAlive = new List<Pawn>();
    }

    public class MapHolder
    {
        public MapPawnsHolder? mapPawns;
    }

    public class MapPawnsHolder
    {
        public List<Pawn> FreeColonists = new List<Pawn>();
    }

    public static class Scribe_Values
    {
        public static string? LastLabel { get; private set; }
        public static int Calls { get; private set; }
        public static bool Loading { get; set; }
        private static readonly Dictionary<string, object?> Values = new();

        public static void Look<T>(ref T value, string label, T? defaultValue = default)
        {
            LastLabel = label;
            Calls++;
            if (Loading)
                value = Values.TryGetValue(label, out var saved) ? (T)saved! : defaultValue!;
            else
                Values[label] = value;
        }

        public static void Reset()
        {
            LastLabel = null;
            Calls = 0;
            Loading = false;
            Values.Clear();
        }
    }

    public static class ModsConfig
    {
        public static bool IsActive(string packageId) => false;
    }

    public class ModSettings
    {
        public virtual void ExposeData() { }
        public void Write() { }
    }

    public static class Current
    {
        public static Game? Game;
    }

    public class Game
    {
        public readonly List<GameComponent> Components = new();
        public T? GetComponent<T>() where T : GameComponent => Components.OfType<T>().FirstOrDefault();
    }

    public class GameComponent
    {
        public GameComponent() { }
        public virtual void ExposeData() { }
    }

    public class ModContentPack { }

    public class Mod
    {
        public Mod(ModContentPack content) { }
        public virtual string SettingsCategory() => "";
        public T GetSettings<T>() where T : ModSettings, new() => new T();
    }

    public static class StringTranslateExtensions
    {
        public static bool CanTranslate(this string s) => true;
        public static string Translate(this string s) => s;
    }
}

namespace RimMind.Bridge.RimChat.Settings
{
    public partial class BridgeRimChatSettings : Verse.ModSettings
    {
        public static void DrawSettingsContent(UnityEngine.Rect rect) { }
    }
}

namespace UnityEngine
{
    public struct Rect { public float x, y, width, height; public Rect(float x, float y, float w, float h) { this.x = x; this.y = y; width = w; height = h; } }
}

namespace RimMind.Presentation.Api
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using RimMind.Application.Common.Interfaces.Context;
    using RimMind.Application.Common.Interfaces.Extension;
    using RimMind.Application.Common.Interfaces.Tools;
    using RimMind.Application.Features.Bridges;
    using RimMind.Application.Features.Tools;
    using RimMind.Domain.ValueObjects;

    public static class RimMindAPI
    {
        public static int ExtensionRegisterCount { get; set; }
        public static int ContextRegisterCount { get; set; }
        public static int ContextUnregisterCount { get; set; }

        public static class Bridges
        {
            private static readonly Action<string> DefaultLogWarn = msg =>
            {
                RimMindErrors.Warn(msg);
                Verse.Log.Warning(msg);
            };

            public static SoftDependencyDetector CreateDetector(
                string packageId,
                IReadOnlyList<string>? candidateTypeNames = null,
                int cacheIntervalTicks = SoftDependencyDetector.DefaultCacheIntervalTicks)
            {
                return new SoftDependencyDetector(
                    packageId,
                    candidateTypeNames,
                    Verse.ModsConfig.IsActive,
                    HarmonyLib.AccessTools.TypeByName,
                    GetCurrentTickSafe,
                    cacheIntervalTicks);
            }

            public static ReflectionAccessor CreateReflectionAccessor(
                string? logPrefix = null,
                Action<string>? warnCallback = null,
                Func<string, Type?>? resolveType = null)
            {
                return new ReflectionAccessor(
                    resolveType ?? HarmonyLib.AccessTools.TypeByName,
                    warnCallback ?? DefaultLogWarn,
                    logPrefix);
            }

            public static ExternalModBridgeCoordinator CreateCoordinator(
                Func<bool> isModAvailable,
                IReadOnlyList<IBridgeModule> modules,
                IReadOnlyList<ISkipCheck>? gates = null,
                Action<string>? logInfo = null,
                Action<string>? logWarn = null,
                string? inactiveMessage = null)
            {
                return new ExternalModBridgeCoordinator(
                    isModAvailable,
                    modules,
                    gates,
                    logInfo ?? Verse.Log.Message,
                    logWarn ?? DefaultLogWarn,
                    gate => Ext.Get<ISkipCheck>().Register(gate),
                    gateId => Ext.Get<ISkipCheck>().Unregister(gateId),
                    inactiveMessage);
            }

            private static int GetCurrentTickSafe()
            {
                try
                {
                    return Verse.Find.TickManager?.TicksGame ?? 0;
                }
                catch
                {
                    return 0;
                }
            }
        }

        public static class Ext
        {
            private static readonly Dictionary<Type, object> Registries = new Dictionary<Type, object>();

            public static IExtensionRegistry<T> Get<T>() where T : class, IExtension
            {
                if (!Registries.TryGetValue(typeof(T), out var registry))
                {
                    registry = new StubRegistry<T>();
                    Registries[typeof(T)] = registry;
                }
                return (IExtensionRegistry<T>)registry;
            }

            public static void Reset() => Registries.Clear();
        }

        public static class Text
        {
            public static string TrOr(string key, string fallback, params object[] args)
            {
                string template = fallback ?? key ?? string.Empty;
                return args == null || args.Length == 0 ? template : string.Format(template, args);
            }
        }

        public static class ToolSet
        {
            private static ToolRegistry _registry = new ToolRegistry();
            public static IToolRegistry Registry => _registry;
            public static void Reset() => _registry = new ToolRegistry();
        }

        public static class MainThreadSnapshot
        {
            public static System.Threading.Tasks.Task<T> GetAsync<T>(
                System.Func<T> readOnMainThread,
                T fallback = default!,
                System.TimeSpan? timeout = null,
                System.Threading.CancellationToken ct = default)
                => System.Threading.Tasks.Task.FromResult(readOnMainThread != null ? readOnMainThread() : fallback);
        }

        public static IExtensionRegistry<T> Extensions<T>() where T : class, IExtension
            => Ext.Get<T>();

        public static void ResetCounts() { ExtensionRegisterCount = 0; ContextRegisterCount = 0; ContextUnregisterCount = 0; }

        // Context 子模块，供 ContextPullBridge 使用
        public static class Context
        {
            public static IContextKeyRegistry ContextKeys => StubContextKeyRegistry.Instance;
        }

        private sealed class StubContextKeyRegistry : IContextKeyRegistry
        {
            public static readonly StubContextKeyRegistry Instance = new StubContextKeyRegistry();
            private readonly List<KeyMeta> _metas = new List<KeyMeta>();

            public void Register(KeyMeta meta)
            {
                _metas.Add(meta);
                RimMindAPI.ContextRegisterCount++;
            }

            public void Register(ContextProviderDef def)
            {
                var meta = new KeyMeta(def.Key, def.Layer, def.Priority,
                    _ => new List<ContextEntry>(), def.OwnerMod ?? string.Empty);
                meta.Def = def;
                _metas.RemoveAll(m => m.Key == def.Key);
                _metas.Add(meta);
                RimMindAPI.ContextRegisterCount++;
            }

            public bool Unregister(string key)
            {
                var removed = _metas.RemoveAll(m => m.Key == key) > 0;
                if (removed) RimMindAPI.ContextUnregisterCount++;
                return removed;
            }

            public IReadOnlyList<KeyMeta> GetAll() => _metas;
            public KeyMeta? Get(string key) => _metas.Find(m => m.Key == key);
            public void Clear() => _metas.Clear();
        }

        private sealed class StubRegistry<T> : IExtensionRegistry<T> where T : class, IExtension
        {
            private readonly List<T> _items = new List<T>();
            public void Register(T extension)
            {
                _items.RemoveAll(x => x.Id == extension.Id);
                _items.Add(extension);
                ExtensionRegisterCount++;
            }
            public bool Unregister(string id) { var item = _items.Find(x => x.Id == id); return item != null && _items.Remove(item); }
            public int UnregisterByOwner(string ownerModId) { var toRemove = _items.Where(x => x.OwnerModId == ownerModId).ToList(); foreach (var item in toRemove) _items.Remove(item); return toRemove.Count; }
            public IReadOnlyList<T> All => _items;
            public T? FindById(string id) => _items.Find(x => x.Id == id);
        }
    }

    public static class RimMindPawnLookup
    {
        public static Verse.Pawn? FindPawnByNumber(int num)
        {
            if (num <= 0) return null;
            foreach (var map in Verse.Find.Maps)
            {
                var pawn = map.mapPawns?.FreeColonists.FirstOrDefault(p => p.thingIDNumber == num);
                if (pawn != null) return pawn;
            }
            return null;
        }

        public static bool IsActiveSpawnedColonist(Verse.Pawn? pawn)
        {
            return pawn != null && !pawn.Dead && !pawn.Destroyed && pawn.Spawned && !pawn.IsPrisoner;
        }
    }
}

// ISettingsTab 真实定义在 RimMind.Presentation.Settings（Core 的 Presentation 层），
// 此处按真实命名空间提供桩，供 RimChatSettingsTab 编译。
namespace RimMind.Presentation.Settings
{
    using RimMind.Application.Common.Interfaces.Extension;

    public interface ISettingsTab : IExtension
    {
        string Label { get; }
        void Draw(UnityEngine.Rect rect);
    }
}

namespace RimMind.Presentation
{
    public abstract class RimMindSubmodBase<TSettings> : Verse.Mod where TSettings : Verse.ModSettings, new()
    {
        public TSettings Settings { get; } = new TSettings();
        protected RimMindSubmodBase(Verse.ModContentPack content) : base(content) { }
        public virtual void DoSettingsWindowContents(UnityEngine.Rect rect) { }
    }
}
