# AGENTS.md — RimMind-Bridge-RimChat

RimMind 与 RimChat 的对话/动作门控、上下文拉取桥。仅编译依赖 Core；
对 RimChat 的访问经过反射，不直接依赖其他 RimMind 子模组。

## 阅读路径

| 任务 | 入口与实际所有者 |
|---|---|
| 加载、设置和扩展注册 | `Source/RimMindBridgeRimChatMod.cs` → `Extensions/` |
| 对话、玩家菜单互斥 | `Source/Bridge/DialogueGate.cs` |
| 动作分类、叙事者事件门控 | `Source/Bridge/ActionGate.cs` |
| 当前游戏事件冷却 | `Source/Cooldown/SharedIncidentCooldown.cs` → `GameComponent_BridgeRimChat.cs` |
| RimChat 历史 → Core Context | `Source/Bridge/ContextPullBridge.cs` → `RimChatApiShim.cs` |
| RimChat 启用检测 | `Source/Detection/RimChatDetector.cs` |
| 设置值与存档 | `Source/Settings/BridgeRimChatSettings.cs` |

## 门控规则

- Chitchat、Auto、PlayerInput 独立开关；玩家菜单与 PlayerInput 共用条件。
- 玩家输入：`enablePlayerInputGate && skipPlayerDialogue && !forceRimMindPlayerDialogue`。
- 动作：`enableActionGate` 关闭或 `forceRimMindActions` 开启时不阻止。
- 外交分类：`adjust_faction`、`trigger_incident`；社交分类：
  `romance_attempt`、`romance_breakup`；招募分类：`recruit_agree`。
- Storyteller 事件是独立门控：`enableActionGate && skipTriggerIncident` 后检查共享冷却；
  不受 `forceRimMindActions` 影响。
- RimChat 未激活时不阻止上述行为。

## 状态与兼容边界

- ModSettings 只保存用户设置；新增设置需同步字段、ExposeData、ApplyDefaults、UI、翻译。
- 上次事件 tick 是 `GameComponent_BridgeRimChat` 的实例字段。
  `SharedIncidentCooldown` 只定位 `Current.Game` 的组件，不持有跨游戏状态。
- 存档键保留 `RimMind_BridgeRimChat_LastIncidentTick`；组件由 Verse 发现 `(Game game)` 构造。
- 新游戏不继承旧游戏冷却；无游戏时冷却不生效。新增 per-game 状态必须由游戏组件拥有。
- 对 RimChat 的反射统一走 `RimChatApiShim`，异常隔离；类型延迟解析并保留 NoInlining。
  manager 获取复用 `TryGetManagerInstance`。
- 所有扩展和 Context Provider 的 owner 为 `RimMindBridgeRimChat`，不要混用带点号名称。
- 设置变化由已注册委托实时读取，不重复注册 SkipCheck。

## Context Provider

通过 `RimMindAPI.Context.ContextKeys.Register(new ContextProviderDef(...))` 注册：

| Key | 来源 | 开关 |
|---|---|---|
| `rimchat_diplomacy` | 外交会话，world-level | `pullDiplomacyHistory` |
| `rimchat_rpg_history` | Pawn RPG 历史 | `pullRpgHistory` |

Pawn 解析复用 `TryFindPawnById`，覆盖世界 pawns 和所有地图，不只当前地图。
Context、门控与事件通知均经过 Core 公共边界。

## 验证

从仓库根目录运行：

```powershell
dotnet test RimMind-Bridge-RimChat/Tests/RimMindBridgeRimChat.Tests.csproj -c Release
dotnet build RimMind-Bridge-RimChat/Source/RimMindBridgeRimChat.csproj -c Release
```

- `Tests/Contracts/RimChatGateContracts.cs`：门控、冷却边界、新游戏隔离和存档恢复。
- `Tests/Contracts/RimChatContextApiContracts.cs`：Context 与反射边界。
- `Tests/Contracts/RimChatCompatibilityContracts.cs`：设置和扩展兼容。
- `Tests/RimChatStubs.cs`：仅替代 Verse/RimChat 等外部边界。

每 mod 全部项目累计少于 1000 个发现用例，每个参数化数据行计数。
游戏 E2E 资源阻塞时不运行、不宣称通过。

## 先询问

修改动作分类、冷却默认值（60000）、RimChat 类型名、新增 ContextPush
或新持久化字段前先确认。禁止对 RimChat 添加编译期引用。
