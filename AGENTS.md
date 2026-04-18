# AGENTS.md — RimMind-Bridge-RimChat

本文件供 AI 编码助手阅读，描述 RimMind-Bridge-RimChat 的架构、代码约定和扩展模式。

## 项目定位

RimMind-Bridge-RimChat 是 RimMind 套件与 RimChat 模组之间的协调层。当两个模组同时激活时，本模组负责：

1. **对话门控**：避免 RimMind-Dialogue 和 RimChat 重复触发对话
2. **动作门控**：避免 RimMind-Actions/Storyteller 和 RimChat 重复执行外交、社交、事件触发等动作
3. **上下文暴露**：将 RimMind 的人格、记忆、叙述者、顾问日志等数据注册为 RimMind Provider，供 RimChat 或其他模组通过 RimMindAPI 读取

本模组通过 `RimMindAPI` 注册 SkipCheck 和 Provider，不依赖 RimChat 的编译期引用，因此 RimChat 未安装时不会报错。

## 源码结构

```
Source/
├── RimMindBridgeRimChatMod.cs   Mod 入口，注册 Harmony、Settings Tab，按条件注册桥接模块
├── Bridge/
│   ├── DialogueGate.cs          对话门控，注册 SkipCheck 防止重复触发
│   ├── ActionGate.cs            动作门控，注册 ActionSkipCheck + IncidentSkipCheck + IncidentCallback
│   └── ContextExposureBridge.cs 上下文暴露，将 RimMind 数据注册为 PawnContextProvider
├── Cooldown/
│   └── SharedIncidentCooldown.cs 事件触发共享冷却，防止 RimMind/Storyteller 与 RimChat 短时间内重复触发事件
├── Detection/
│   └── RimChatDetector.cs       检测 RimChat 是否激活（带缓存）
└── Settings/
    └── BridgeRimChatSettings.cs 模组设置（对话门控 + 动作门控 + 上下文暴露）
```

## 关键类与 API

### RimChatDetector

检测 RimChat 模组状态，带缓存：

```csharp
static class RimChatDetector {
    const string RimChatPackageId = "yancy.rimchat";

    bool IsRimChatActive  // RimChat 模组是否激活（6000 tick 缓存）
    void InvalidateCache() // 手动刷新缓存
}
```

### DialogueGate

对话门控，防止 RimMind-Dialogue 和 RimChat 同时触发对话：

```csharp
static class DialogueGate {
    bool ShouldSkipDialogue(Pawn pawn, string triggerType)
    // triggerType: "PlayerInput" 时检查 skipPlayerDialogue

    bool ShouldSkipFloatMenuOption()
    // 判断是否跳过 RimMind 的"与X对话"浮动菜单

    void RegisterSkipChecks()
    // 注册到 RimMindAPI.RegisterDialogueSkipCheck / RegisterFloatMenuSkipCheck

    void UnregisterSkipChecks()
    // 清理注册
}
```

门控逻辑：

| 条件 | 跳过条件 |
|------|---------|
| `"PlayerInput"` 对话 | `enableDialogueGate && skipPlayerDialogue && !forceRimMindPlayerDialogue` |
| 浮动菜单 | `enableDialogueGate && skipPlayerDialogue && !forceRimMindPlayerDialogue` |

### ActionGate

动作门控，防止 RimMind-Actions/Storyteller 和 RimChat 重复执行动作：

```csharp
static class ActionGate {
    bool ShouldSkipAction(string intentId)
    // 检查动作是否应被跳过

    bool ShouldSkipStorytellerIncident()
    // 检查 Storyteller 事件是否应被跳过（含冷却检查）

    void Register()
    // 注册到 RimMindAPI.RegisterActionSkipCheck / RegisterIncidentExecutedCallback / RegisterStorytellerIncidentSkipCheck

    void Unregister()
    // 清理注册
}
```

动作分类：

| 分类 | 动作 ID | 设置开关 |
|------|---------|---------|
| 外交 | `adjust_faction`, `trigger_incident` | skipDiplomacyActions |
| 社交 | `romance_accept`, `romance_breakup` | skipSocialActions |
| 招募 | `recruit_agree` | skipRecruitAgree |
| 事件触发 | Storyteller incident | skipTriggerIncident + SharedIncidentCooldown |

门控逻辑：

```
ShouldSkipAction:
  RimChat 未激活 → false
  enableActionGate 关闭 → false
  forceRimMindActions 开启 → false（强制 RimMind 执行所有动作）
  skipDiplomacyActions && intentId ∈ DiplomacyActions → true
  skipSocialActions && intentId ∈ SocialActions → true
  skipRecruitAgree && intentId == "recruit_agree" → true
  否则 → false

ShouldSkipStorytellerIncident:
  RimChat 未激活 → false
  enableActionGate 关闭 → false
  skipTriggerIncident 关闭 → false
  SharedIncidentCooldown.IsOnCooldown(incidentCooldownTicks) → true/false
```

### SharedIncidentCooldown

事件触发共享冷却，防止 RimMind-Storyteller 和 RimChat 短时间内重复触发事件：

```csharp
static class SharedIncidentCooldown {
    void RecordIncident()          // 记录事件触发时间
    bool IsOnCooldown(int cooldownTicks) // 检查是否在冷却中
    int LastIncidentTick { get; }  // 上次事件触发 tick
    void Reset()                   // 重置冷却
}
```

### ContextExposureBridge

将 RimMind 数据注册为 RimMind PawnContextProvider，供 RimChat 或其他模组通过 `RimMindAPI` 读取：

```csharp
static class ContextExposureBridge {
    void Register()    // 根据设置注册各 Provider
    void Unregister()  // 清理所有注册（RimMindAPI.UnregisterModProviders）
}
```

注册的 Provider：

| Category | 数据来源 | 优先级 | 设置开关 |
|----------|---------|--------|---------|
| `rimmind_personality` | AIPersonalityWorldComponent（description + workTendencies + socialTendencies + aiNarrative） | PriorityMemory | exposePersonality |
| `rimmind_memory` | RimMindMemoryWorldComponent.PawnStore（active 最多 5 条 + dark 全部） | PriorityMemory | exposeMemory |
| `rimmind_storyteller` | RimMindMemoryWorldComponent.NarratorStore（active 最多 5 条） | PriorityAuxiliary | exposeStoryteller |
| `rimmind_advisor_log` | AdvisorHistoryStore（最多 5 条，格式：action: reason (result)） | PriorityAuxiliary | exposeAdvisorLog |

所有 Provider 注册时使用 ModId `"RimMind.Bridge.RimChat"`，卸载时通过 `UnregisterModProviders` 一次性清理。

### BridgeRimChatSettings

```csharp
class BridgeRimChatSettings : ModSettings {
    // 对话门控
    bool enableDialogueGate;          // 默认 true
    bool skipPlayerDialogue;          // 默认 true
    bool forceRimMindPlayerDialogue;  // 默认 false

    // 动作门控
    bool enableActionGate;            // 默认 true
    bool skipDiplomacyActions;        // 默认 true
    bool skipTriggerIncident;         // 默认 true
    bool skipSocialActions;           // 默认 false
    bool skipRecruitAgree;            // 默认 false
    int incidentCooldownTicks;        // 默认 60000（1 游戏天）
    bool forceRimMindActions;         // 默认 false

    // 上下文暴露
    bool enableContextExposure;       // 默认 true
    bool exposePersonality;           // 默认 true
    bool exposeMemory;                // 默认 true
    bool exposeStoryteller;           // 默认 false
    bool exposeAdvisorLog;            // 默认 false

    static BridgeRimChatSettings Get();
    static void DrawSettingsContent(Rect inRect);
}
```

## 数据流

```
RimMind 子模组数据                RimMind 上下文系统
┌──────────────────┐             ┌──────────────────┐
│ Personality      │──Provider──→│ rimmind_personality
│ Memory           │──Provider──→│ rimmind_memory
│ Storyteller      │──Provider──→│ rimmind_storyteller
│ Advisor          │──Provider──→│ rimmind_advisor_log
└──────────────────┘             └──────────────────┘
                                        │
                              RimChat 通过 RimMindAPI 读取

RimMind-Dialogue 触发  ──DialogueGate──→  跳过/放行
RimMind-Actions 执行   ──ActionGate────→  跳过/放行
RimMind-Storyteller    ──ActionGate────→  跳过/放行（含冷却）
```

## 初始化流程

```
RimMindBridgeRimChatMod 构造函数
    │
    ├── GetSettings<BridgeRimChatSettings>()
    ├── Harmony("mcocdaa.RimMindBridgeRimChat").PatchAll()
    ├── RimMindAPI.RegisterSettingsTab("bridge_rimchat", ...)
    │
    ├── RimChatDetector.IsRimChatActive?
    │       │
    │       ├── No  → Log + 跳过所有桥接模块
    │       │
    │       └── Yes → DialogueGate.RegisterSkipChecks()
    │               ActionGate.Register()
    │               ContextExposureBridge.Register()
```

## 代码约定

### 命名空间

| 命名空间 | 目录 | 职责 |
|---------|------|------|
| `RimMind.Bridge.RimChat` | Source/ 根目录 | Mod 入口 |
| `RimMind.Bridge.RimChat.Bridge` | Bridge/ | 桥接模块 |
| `RimMind.Bridge.RimChat.Cooldown` | Cooldown/ | 冷却管理 |
| `RimMind.Bridge.RimChat.Detection` | Detection/ | RimChat 检测 |
| `RimMind.Bridge.RimChat.Settings` | Settings/ | 设置 |

### ModId

所有 RimMindAPI 注册使用统一 ModId：`"RimMind.Bridge.RimChat"`

### Harmony

- Harmony ID：`mcocdaa.RimMindBridgeRimChat`
- 当前无 Harmony Patch（预留）

### 构建

| 配置项 | 值 |
|--------|-----|
| 目标框架 | `net48` |
| C# 语言版本 | 9.0 |
| Nullable | enable |
| RimWorld 版本 | 1.6 |
| 输出路径 | `../1.6/Assemblies/` |
| 部署 | 设置 `RIMWORLD_DIR` 环境变量后自动部署 |
| NuGet 依赖 | `Krafs.Rimworld.Ref 1.6.*-*`, `Lib.Harmony.Ref 2.*`, `Newtonsoft.Json 13.0.*` |
| 编译期引用 | RimMindCore, RimMindDialogue, RimMindPersonality, RimMindMemory, RimMindStoryteller, RimMindAdvisor, RimMindActions（均为 Private=false） |
| 无编译期引用 | RimChat（纯检测，不调用 RimChat API） |

### 加载顺序

```
Harmony → yancy.rimchat → RimMind-Core → RimMind 子模组 → RimMind-Bridge-RimChat
```

### UI 本地化

所有 UI 文本通过 `Languages/ChineseSimplified/DefInjected/RimMind.BridgeRimChat.Settings.xml` 的 Keyed 翻译，禁止硬编码中文。

### 设置 UI

通过 `RimMindAPI.RegisterSettingsTab` 注册到 Core 的多分页设置界面，Tab 标签为 "Bridge (RimChat)"。使用 `SettingsUIHelper` 辅助工具类绘制。

## 与 RimTalk Bridge 的区别

| 特性 | RimChat Bridge | RimTalk Bridge |
|------|---------------|----------------|
| 目标模组 | RimChat (`yancy.rimchat`) | RimTalk (`cj.rimtalk`) |
| 对话门控 | 仅 PlayerInput | Chitchat + Auto + PlayerInput |
| 动作门控 | 有（外交/社交/招募/事件） | 无 |
| 事件冷却 | 有（SharedIncidentCooldown） | 无 |
| 上下文方向 | RimMind → RimMind Provider（RimChat 读取） | RimMind → RimTalk API（推送）+ RimTalk → RimMind（拉取） |
| RimChat API 调用 | 无（仅检测激活状态） | 有（反射调用 RimTalkPromptAPI） |
| 人格推送 | 无（通过 Provider 暴露） | 有（PersonaPushBridge + Hook） |

## 扩展指南

### 新增动作门控

1. 在 `ActionGate` 中添加动作 ID 到对应 HashSet，或新增 HashSet
2. 在 `BridgeRimChatSettings` 中添加对应开关
3. 在语言文件中添加翻译键

### 新增上下文暴露

1. 在 `ContextExposureBridge` 中添加注册方法
2. 使用 `RimMindAPI.RegisterPawnContextProvider` 注册
3. 在 `BridgeRimChatSettings` 中添加对应开关
4. 在语言文件中添加翻译键

### 新增对话门控类型

1. 在 `DialogueGate.ShouldSkipDialogue` 中添加 triggerType 分支
2. 在 `BridgeRimChatSettings` 中添加对应开关
3. 在语言文件中添加翻译键
