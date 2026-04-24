# CLAUDE.md — RimMind-Bridge-RimChat

本文件供 AI 编码助手阅读，描述 RimMind-Bridge-RimChat 的架构、代码约定和扩展模式。

## 项目定位

RimMind-Bridge-RimChat 是 RimMind 套件与 RimChat 模组之间的协调层。当两个模组同时激活时，本模组负责：

1. **对话门控**：避免 RimMind-Dialogue 和 RimChat 重复触发玩家对话
2. **动作门控**：避免 RimMind-Actions 和 RimChat 重复执行外交、社交、招募等动作；避免 RimMind-Storyteller 和 RimChat 重复触发事件
3. **上下文拉取**：从 RimChat 拉取外交/RPG对话历史，注册为 RimMind Provider，使 RimMind 能感知 RimChat 的对话内容

本模组通过 `RimMindAPI` 注册 SkipCheck 和 Provider，不依赖 RimChat 的编译期引用，因此 RimChat 未安装时不会报错。

## 源码结构

```
Source/
├── RimMindBridgeRimChatMod.cs   Mod 入口，注册 Settings Tab，按条件注册桥接模块
├── Bridge/
│   ├── DialogueGate.cs          对话门控，注册 SkipCheck 防止重复触发
│   ├── ActionGate.cs            动作门控，注册 ActionSkipCheck + IncidentSkipCheck + IncidentCallback
│   └── ContextPullBridge.cs     上下文拉取，从 RimChat 读取对话历史注册为 RimMind Provider
├── Cooldown/
│   └── SharedIncidentCooldown.cs 事件触发共享冷却，防止 RimMind-Storyteller 与 RimChat 短时间内重复触发事件
├── Detection/
│   └── RimChatDetector.cs       检测 RimChat 是否激活（带缓存）
└── Settings/
    └── BridgeRimChatSettings.cs 模组设置（对话门控 + 动作门控 + 上下文拉取）
```

## 关键类与 API

### RimChatDetector

检测 RimChat 模组状态，带缓存：

```csharp
static class RimChatDetector {
    const string RimChatPackageId = "yancy.rimchat";

    bool IsRimChatActive  // RimChat 模组是否激活（6000 tick 缓存）
    void InvalidateCache() // 手动刷新缓存（当前无调用者）
}
```

### DialogueGate

对话门控，防止 RimMind-Dialogue 和 RimChat 同时触发玩家对话：

```csharp
static class DialogueGate {
    bool ShouldSkipDialogue(Pawn pawn, string triggerType)
    bool ShouldSkipFloatMenuOption()
    void RegisterSkipChecks()
    void UnregisterSkipChecks() // 当前无调用者
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
    bool ShouldSkipStorytellerIncident()
    void Register()
    void Unregister() // 当前无调用者
}
```

动作分类：

| 分类 | 动作 ID | 设置开关 |
|------|---------|---------|
| 外交 | `adjust_faction`, `trigger_incident` | skipDiplomacyActions |
| 社交 | `romance_accept`, `romance_breakup` | skipSocialActions |
| 招募 | `recruit_agree` | skipRecruitAgree |
| 叙事者事件 | Storyteller incident | skipTriggerIncident + SharedIncidentCooldown |

### SharedIncidentCooldown

事件触发共享冷却：

```csharp
static class SharedIncidentCooldown {
    void RecordIncident()
    bool IsOnCooldown(int cooldownTicks)
    int LastIncidentTick { get; }  // 当前无读取者
    void Reset()                   // 当前无调用者
}
```

### ContextPullBridge

从 RimChat 拉取对话历史，注册为 RimMind Provider：

```csharp
static class ContextPullBridge {
    void Register()    // 根据设置注册各 Provider
    void Unregister()  // 清理所有注册（RimMindAPI.UnregisterModProviders）
    void Refresh()     // Unregister + Register，设置变更时调用
}
```

注册的 Provider：

| Category | 数据来源 | 类型 | 优先级 | 设置开关 |
|----------|---------|------|--------|---------|
| `rimchat_diplomacy` | GameComponent_DiplomacyManager.dialogueSessions（反射） | StaticProvider | PriorityAuxiliary | pullDiplomacyHistory |
| `rimchat_rpg_history` | RpgNpcDialogueArchiveManager._archiveCache（反射） | PawnContextProvider | PriorityMemory | pullRpgHistory |

### BridgeRimChatSettings

```csharp
class BridgeRimChatSettings : ModSettings {
    bool enableDialogueGate;          // 默认 true
    bool skipPlayerDialogue;          // 默认 true
    bool forceRimMindPlayerDialogue;  // 默认 false
    bool enableActionGate;            // 默认 true
    bool skipDiplomacyActions;        // 默认 true
    bool skipTriggerIncident;         // 默认 true
    bool skipSocialActions;           // 默认 false
    bool skipRecruitAgree;            // 默认 false
    int incidentCooldownTicks;        // 默认 60000，Slider 6000~180000，步进 1500
    bool forceRimMindActions;         // 默认 false
    bool enableContextPull;           // 默认 true
    bool pullDiplomacyHistory;        // 默认 true
    bool pullRpgHistory;              // 默认 false
}
```

## 初始化流程

```
RimMindBridgeRimChatMod 构造函数
    │
    ├── GetSettings<BridgeRimChatSettings>()
    ├── RimMindAPI.RegisterSettingsTab("bridge_rimchat", ...)
    │
    ├── RimChatDetector.IsRimChatActive?
    │       ├── No  → Log + 跳过所有桥接模块
    │       └── Yes → DialogueGate.RegisterSkipChecks()
    │               ActionGate.Register()
    │               ContextPullBridge.Register()
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

### 构建

| 配置项 | 值 |
|--------|-----|
| 目标框架 | `net48` |
| C# 语言版本 | 9.0 |
| Nullable | enable |
| RimWorld 版本 | 1.6 |
| NuGet 依赖 | `Krafs.Rimworld.Ref 1.6.*-*`, `Lib.Harmony.Ref 2.*` |
| 编译期引用 | RimMindCore（Private=false） |
| 无编译期引用 | RimChat（纯反射读取） |

### 加载顺序

```
Harmony → yancy.rimchat → RimMind-Core → RimMind 子模组 → RimMind-Bridge-RimChat
```

### UI 本地化

翻译键共 31 个，覆盖 3 个设置分区 + Mod 入口。禁止硬编码中文。

### 设置变更检测

仅 ContextPull 相关设置变更时触发 `ContextPullBridge.Refresh()`。DialogueGate 和 ActionGate 的设置通过委托实时读取，无需额外检测。

## 扩展指南

### 新增动作门控

1. 在 `ActionGate` 中添加动作 ID 到对应 HashSet，或新增 HashSet
2. 在 `BridgeRimChatSettings` 中添加对应开关
3. 在语言文件中添加翻译键
4. 更新 `EstimateHeight` 中的高度计算

### 新增上下文拉取

1. 在 `ContextPullBridge` 中添加反射读取方法
2. 使用 `RimMindAPI.RegisterPawnContextProvider` 或 `RegisterStaticProvider` 注册
3. 在 `BridgeRimChatSettings` 中添加对应开关
4. 在语言文件中添加翻译键

### 新增对话门控类型

1. 在 `DialogueGate.ShouldSkipDialogue` 中添加 triggerType 分支
2. 在 `BridgeRimChatSettings` 中添加对应开关
3. 在语言文件中添加翻译键
4. 更新 `EstimateHeight` 中的高度计算
