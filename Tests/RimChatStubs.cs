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

        public static void Look<T>(ref T value, string label, T? defaultValue = default)
        {
            LastLabel = label;
            Calls++;
        }

        public static void Reset()
        {
            LastLabel = null;
            Calls = 0;
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

    public class Game { }

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

namespace RimMind.Application.Common.Interfaces.Extension
{
    public interface IExtension { string Id { get; } string OwnerModId { get; } }
    public interface IExtensionRegistry<T> where T : class, IExtension
    {
        void Register(T extension);
        bool Unregister(string id);
        int UnregisterByOwner(string ownerModId);
        System.Collections.Generic.IReadOnlyList<T> All { get; }
        T? FindById(string id);
    }
    // ISettingsTab 真实定义在 RimMind.Presentation.Settings（见文件末尾），此处不再重复定义以避免命名冲突。
    public interface IToggleBehavior : IExtension { bool IsActive { get; } void Toggle(); }
    public interface IDialogueTrigger : IExtension { void Trigger(Verse.Pawn pawn, string context, Verse.Pawn? recipient); }
    public interface IModCooldown : IExtension { int CooldownTicks { get; } }
    public enum SkipCheckKind { Dialogue, FloatMenu, Action, StorytellerIncident }
    public readonly struct SkipCheckArgs
    {
        public object? Pawn { get; init; }
        public string? Trigger { get; init; }
        public string? IntentId { get; init; }
    }
    public interface ISkipCheck : IExtension { SkipCheckKind Kind { get; } bool ShouldSkip(in SkipCheckArgs args); }
    public interface IIncidentExecutedListener : IExtension { void OnIncidentExecuted(); }
}

namespace UnityEngine
{
    public struct Rect { public float x, y, width, height; public Rect(float x, float y, float w, float h) { this.x = x; this.y = y; width = w; height = h; } }
}

// ── Application 层接口桩（ContextPullBridge 依赖）──
namespace RimMind.Application.Common.Interfaces.Context
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using RimMind.Domain.ValueObjects;

    /// <summary>
    /// 异步上下文 Provider 定义（桩，与源码签名一致）
    /// </summary>
    public sealed class ContextProviderDef
    {
        public string Key { get; }
        public ContextLayer Layer { get; }
        public float Priority { get; }
        public string? OwnerMod { get; }
        public Func<ProviderContext, CancellationToken, Task<string?>> Provider { get; }
        public int StalenessTicks { get; }
        public IReadOnlyList<string>? InvalidationTriggers { get; }

        public ContextProviderDef(
            string key,
            ContextLayer layer,
            float priority,
            Func<ProviderContext, CancellationToken, Task<string?>> provider,
            string? ownerMod = null,
            int stalenessTicks = 0,
            IReadOnlyList<string>? invalidationTriggers = null)
        {
            Key = key ?? throw new ArgumentNullException(nameof(key));
            Layer = layer;
            Priority = priority;
            Provider = provider ?? throw new ArgumentNullException(nameof(provider));
            OwnerMod = ownerMod;
            StalenessTicks = stalenessTicks;
            InvalidationTriggers = invalidationTriggers;
        }
    }

    /// <summary>
    /// 上下文 Provider 参数（桩）
    /// </summary>
    public sealed record ProviderContext
    {
        public string Scenario { get; init; }
        public string TraceId { get; init; }
        public int PawnId { get; init; }
        public string? NpcId { get; init; }
        public int? MapId { get; init; }
        public IReadOnlyDictionary<string, object?>? Hints { get; init; }

        public ProviderContext(string scenario, string traceId)
        {
            Scenario = scenario ?? throw new ArgumentNullException(nameof(scenario));
            TraceId = traceId ?? throw new ArgumentNullException(nameof(traceId));
        }
    }

    /// <summary>
    /// 上下文 Key 注册表接口（桩）
    /// </summary>
    public interface IContextKeyRegistry
    {
        void Register(KeyMeta meta);
        void Register(ContextProviderDef def);
        bool Unregister(string key);
        IReadOnlyList<KeyMeta> GetAll();
        KeyMeta? Get(string key);
        void Clear();
    }
}

namespace RimMind.Presentation.Api
{
    using RimMind.Application.Common.Interfaces.Extension;
    using RimMind.Application.Common.Interfaces.Context;
    using RimMind.Domain.ValueObjects;
    using System.Collections.Generic;

    public static class RimMindAPI
    {
        public static int ExtensionRegisterCount { get; set; }
        public static int ContextRegisterCount { get; set; }
        public static int ContextUnregisterCount { get; set; }

        public static IExtensionRegistry<T> Extensions<T>() where T : class, IExtension
            => new StubRegistry<T>();

        public static bool ShouldSkipDialogue(Verse.Pawn pawn, string trigger) => false;
        public static bool ShouldSkipFloatMenu() => false;
        public static bool ShouldSkipAction(string intentId) => false;
        public static bool ShouldSkipStorytellerIncident() => false;
        public static bool CanTriggerDialogue => false;
        public static void TriggerDialogue(Verse.Pawn pawn, string context, Verse.Pawn? recipient = null) { }
        public static void NotifyIncidentExecuted() { }

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
                // 用 ContextProviderDef 的信息构造 KeyMeta
                var meta = new KeyMeta(def.Key, def.Layer, def.Priority,
                    _ => new List<ContextEntry>(), def.OwnerMod ?? string.Empty);
                meta.Def = def;
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
            public void Register(T extension) { _items.Add(extension); ExtensionRegisterCount++; }
            public bool Unregister(string id) { var item = _items.Find(x => x.Id == id); return item != null && _items.Remove(item); }
            public int UnregisterByOwner(string ownerModId) { var toRemove = _items.Where(x => x.OwnerModId == ownerModId).ToList(); foreach (var item in toRemove) _items.Remove(item); return toRemove.Count; }
            public IReadOnlyList<T> All => _items;
            public T? FindById(string id) => _items.Find(x => x.Id == id);
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
