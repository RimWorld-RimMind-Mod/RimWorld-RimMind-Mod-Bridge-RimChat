using System.Linq;

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

    public class Pawn { }

    public static class Find
    {
        public static TickManager TickManager = new TickManager();
    }

    public class TickManager
    {
        public int TicksGame = 100000;
    }

    public static class Scribe_Values
    {
        public static void Look<T>(ref T value, string label, T? defaultValue = default) { }
    }

    public class ModSettings
    {
        public virtual void ExposeData() { }
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

namespace RimMind.Bridge.RimChat.Settings
{
    public class BridgeRimChatSettings : Verse.ModSettings
    {
        public bool enablePlayerInputGate = true;
        public bool enableChitchatGate = true;
        public bool enableAutoGate = true;
        public bool skipPlayerDialogue = true;
        public bool forceRimMindPlayerDialogue = false;

        public bool enableActionGate = true;
        public bool skipDiplomacyActions = true;
        public bool skipTriggerIncident = true;
        public bool skipSocialActions = false;
        public bool skipRecruitAgree = false;
        public int incidentCooldownTicks = 60000;
        public bool forceRimMindActions = false;

        public bool enableContextPull = true;
        public bool pullDiplomacyHistory = true;
        public bool pullRpgHistory = false;

        private static BridgeRimChatSettings? _instance;
        public static BridgeRimChatSettings Get() => _instance ??= new BridgeRimChatSettings();

        public BridgeRimChatSettings() { _instance = this; }

        public static void Reset() { _instance = null; }
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
    public interface ISettingsTab : IExtension { string Label { get; } void Draw(UnityEngine.Rect rect); }
    public interface IToggleBehavior : IExtension { bool IsActive { get; } void Toggle(); }
    public interface IDialogueTrigger : IExtension { void Trigger(Verse.Pawn pawn, string context, Verse.Pawn? recipient); }
    public interface IModCooldown : IExtension { int CooldownTicks { get; } }
    public enum SkipCheckKind { Dialogue, FloatMenu, Action, StorytellerIncident }
    public readonly struct SkipCheckArgs { public readonly Verse.Pawn? Pawn; public readonly string? Trigger; public readonly string? IntentId; }
    public interface ISkipCheck : IExtension { SkipCheckKind Kind { get; } bool ShouldSkip(in SkipCheckArgs args); }
    public interface IIncidentExecutedListener : IExtension { void OnIncidentExecuted(); }
}

namespace UnityEngine
{
    public struct Rect { public float x, y, width, height; public Rect(float x, float y, float w, float h) { this.x = x; this.y = y; width = w; height = h; } }
}

namespace RimMind.Presentation
{
    using RimMind.Application.Common.Interfaces.Extension;
    using System.Collections.Generic;

    public static class RimMindAPI
    {
        public static int ExtensionRegisterCount { get; set; }

        public static IExtensionRegistry<T> Extensions<T>() where T : class, IExtension
            => new StubRegistry<T>();

        public static bool ShouldSkipDialogue(Verse.Pawn pawn, string trigger) => false;
        public static bool ShouldSkipFloatMenu() => false;
        public static bool ShouldSkipAction(string intentId) => false;
        public static bool ShouldSkipStorytellerIncident() => false;
        public static bool CanTriggerDialogue => false;
        public static void TriggerDialogue(Verse.Pawn pawn, string context, Verse.Pawn? recipient = null) { }
        public static void NotifyIncidentExecuted() { }

        public static void ResetCounts() { ExtensionRegisterCount = 0; }

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
