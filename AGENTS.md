# AGENTS.md — RimMind-Bridge-RimChat

RimMind 与 RimChat 模组协调层，对话/动作门控 + 上下文拉取。

## 项目定位

纯反射(无编译期引用):
- **DialogueGate**: 注册SkipCheck防止与RimChat重复触发对话(Chitchat/Auto/PlayerInput三种triggerType独立门控)；`ShouldSkipFloatMenuOption` 委托给私有 `ShouldSkipPlayerInput`，不再重复表达式
- **ActionGate**: 注册ActionSkipCheck防止重复执行外交(`DiplomacyActions`)、社交(`SocialActions`)、招募(`RecruitActions = { recruit_agree }`)；注册IncidentSkipCheck+SharedIncidentCooldown防止叙事者事件重复
- **ContextPullBridge**: 通过RimChatApiShim反射读取RimChat外交/RPG对话历史，注册为RimMind ContextKey(rimchat_diplomacy/rimchat_rpg_history)；`TryFindPawnById` 遍历 `Find.Maps`（非 `Find.CurrentMap`）以覆盖非当前地图的殖民者
- **RimChatApiShim**: 反射封装层，统一处理RimChat类型解析、字段访问、manager单例获取(`TryGetManagerInstance`)，延迟解析+NoInlining防止类型加载异常

## 构建

| 项 | 值 |
|----|-----|
| Target | net48, C#9.0, Nullable enable |
| Output | `../1.6/Assemblies/` |
| Assembly | RimMindBridgeRimChat |
| 依赖 | RimMindCore.dll, Krafs.Rimworld.Ref, Lib.Harmony.Ref |
| 无编译期引用 | RimChat(纯反射)，RimMind-Dialogue/Actions/Storyteller(运行时委托) |
| 测试 | Tests/ 目录，xunit，net10.0，纯逻辑无 RimWorld 依赖 |

## 源码结构

```
Source/
├── RimMindBridgeRimChatMod.cs          Mod入口(注册Settings/SkipCheck/Listener/ContextPull)
├── Bridge/
│   ├── DialogueGate.cs                 对话门控(ShouldSkipDialogue/ShouldSkipFloatMenuOption/ShouldSkipPlayerInput)
│   ├── ActionGate.cs                   动作门控(DiplomacyActions/SocialActions/RecruitActions + ShouldSkipAction/ShouldSkipStorytellerIncident)
│   ├── ContextPullBridge.cs            上下文拉取(rimchat_diplomacy+rimchat_rpg_history, TryFindPawnById遍历Find.Maps)
│   └── RimChatApiShim.cs               反射封装(类型解析+字段访问+TryGetManagerInstance)
├── Cooldown/
│   ├── GameComponent_BridgeRimChat.cs  per-game状态承载(RimWorld反射发现, ExposeData调用SharedIncidentCooldown)
│   └── SharedIncidentCooldown.cs       事件共享冷却(ExposeData为internal,仅GameComponent调用)
├── Detection/RimChatDetector.cs        检测RimChat激活(仅IsRimChatActive+RimChatPackageId,无API可用性检查)
├── Extensions/                         ISkipCheck/IIncidentExecutedListener/ISettingsTab实现
│   ├── RimChatDialogueSkipCheck.cs         无_mod字段,无参构造
│   ├── RimChatFloatMenuSkipCheck.cs
│   ├── RimChatActionSkipCheck.cs
│   ├── RimChatStorytellerIncidentSkipCheck.cs
│   ├── RimChatIncidentExecutedListener.cs
│   └── RimChatSettingsTab.cs
├── Debug/BridgeRimChatDebugActions.cs  开发者模式Autotests动作
└── Settings/BridgeRimChatSettings.cs   15项设置(ApplyDefaults统一默认值,供"恢复默认"按钮)

Tests/  (net10.0, xunit, 纯逻辑)
├── RimChatStubs.cs                          测试桩(AccessTools/Log/Find/RimChatDetector/BridgeRimChatSettings/ContextProviderDef)
├── RimChatApiShimTests.cs                  RimChatApiShim单元测试
├── DialogueGateTests.cs                    DialogueGate门控测试
├── DialogueGateFloatMenuConsistencyTests.cs ShouldSkipFloatMenuOption与ShouldSkipPlayerInput一致性
├── ActionGateTests.cs                      ActionGate门控测试
├── ActionGateClassificationTests.cs        DiplomacyActions/SocialActions/RecruitActions分类边界
├── SharedIncidentCooldownTests.cs          冷却记录/判定
├── GameComponentBridgeRimChatTests.cs      GameComponent ExposeData委托到SharedIncidentCooldown
├── ContextPullBridgeTests.cs               Provider注册/卸载/Refresh
├── ContextPullBridgePawnLookupTests.cs     TryFindPawnById跨地图/世界pawns查找
├── BridgeRimChatSettingsTests.cs           设置持久化(ExposeData)
├── BridgeRimChatSettingsDefaultsTests.cs   ApplyDefaults重置全部字段
├── RimChatDetectorTests.cs                 RimChat激活检测
├── SkipCheckExtensionTests.cs              SkipCheck注册/Id/Kind
└── OwnerModIdConsistencyTests.cs           所有Extension+ContextPullBridge的OwnerModId统一性
```

## 门控逻辑

```
ShouldSkipDialogue(pawn, triggerType):
  !RimChatDetector.IsRimChatActive → false
  Chitchat    → enableChitchatGate
  Auto        → enableAutoGate
  PlayerInput → ShouldSkipPlayerInput(settings)   // 委托私有方法
  _           → false

ShouldSkipFloatMenuOption():
  !RimChatDetector.IsRimChatActive → false
  → ShouldSkipPlayerInput(settings)               // 委托,不再重复表达式

ShouldSkipPlayerInput(settings):  // private static
  enablePlayerInputGate && skipPlayerDialogue && !forceRimMindPlayerDialogue

ShouldSkipAction(intentId):
  !RimChatDetector.IsRimChatActive → false
  !enableActionGate || forceRimMindActions → false
  skipDiplomacyActions && intentId∈DiplomacyActions{adjust_faction, trigger_incident} → true
  skipSocialActions    && intentId∈SocialActions{romance_attempt, romance_breakup}    → true
  skipRecruitAgree     && intentId∈RecruitActions{recruit_agree}                      → true
  else → false

ShouldSkipStorytellerIncident():
  !RimChatDetector.IsRimChatActive → false
  !enableActionGate || !skipTriggerIncident → false
  → SharedIncidentCooldown.IsOnCooldown(incidentCooldownTicks)
```

`DiplomacyActions` / `SocialActions` / `RecruitActions` 均为 `private static readonly HashSet<string>`，分类逻辑集中在 `ActionGate`，扩展时新增 HashSet + 设置开关 + ShouldSkipAction 分支。

## Provider注册 (ModId: `"RimMindBridgeRimChat"`)

| Category | 数据来源 | 类型 | 设置开关 |
|----------|---------|------|---------|
| rimchat_diplomacy | DiplomacyManager.dialogueSessions(反射) | Static | pullDiplomacyHistory |
| rimchat_rpg_history | RpgNpcDialogueArchiveManager._archiveCache(反射) | Pawn | pullRpgHistory |

使用 `ContextKeyRegistry.Register` 注册，provider 统一为 `Func<ProviderContext, CancellationToken, Task<string?>>`（静态上下文忽略 PawnId，Pawn 上下文通过 `TryFindPawnById` 解析）。

`ContextPullBridge.ModId` 为 `private const string`，与所有 Extension 的 `OwnerModId` 保持一致（由 `OwnerModIdConsistencyTests` 守护）。

## RimChatApiShim 反射封装

```
延迟解析: EnsureResolved() → ResolveTypes() [NoInlining]
  → AccessTools.TypeByName 解析3个类型:
    - RimChat.API.RimChatAPI
    - RimChat.DiplomacySystem.GameComponent_DiplomacyManager
    - RimChat.Memory.RpgNpcDialogueArchiveManager

类型暴露属性(均触发EnsureResolved):
  ApiType / DiplomacyManagerType / RpgArchiveManagerType  → Type?

工具方法:
  GetStaticPropertyValue(Type, propertyName) → object?
  GetInstanceFieldValue(instance, fieldName, BindingFlags) → object?
  TryGetManagerInstance(Type? managerType, instancePropertyName = "Instance") → object?
    // 统一manager单例获取: 封装 GetStaticPropertyValue + null检查 + RimMindErrors.Warn
    // BuildDiplomacyContext / BuildRpgContext 共用,避免重复null处理代码
```

## 持久化边界 (per-mod vs per-game)

| 状态类别 | 载体 | ExposeData 可见性 | 说明 |
|---------|------|------------------|------|
| 用户设置(15项开关/冷却ticks) | `BridgeRimChatSettings : ModSettings` | `public override` | per-mod，跟随 ModSettings 写入 `%RimWorldConfig%/ModSettings.xml` |
| 运行时事件状态(上次事件tick) | `SharedIncidentCooldown._lastIncidentTick` | `internal static` | per-game，由 `GameComponent_BridgeRimChat.ExposeData` 委托持久化到存档 |

`SharedIncidentCooldown.ExposeData` 为 `internal`，仅 `GameComponent_BridgeRimChat` 调用；外部代码禁止直接调用。`GameComponent_BridgeRimChat` 通过 RimWorld 反射自动发现 `(Game game)` 构造函数，无需手动注册。

## 可扩展性

### 新增对话触发门控

1. 在 `BridgeRimChatSettings` 新增 `enableXxxGate` 字段（同时在 `ExposeData` + `ApplyDefaults` + UI 三处同步）。
2. 在 `DialogueGate.ShouldSkipDialogue` 的 switch 增加 case，或在 `ShouldSkipPlayerInput` 增加条件。
3. 若是新 triggerType，确认上游（RimMind-Dialogue）已发出该 triggerType 字符串。
4. 在 `DialogueGateTests` / `DialogueGateFloatMenuConsistencyTests` 补充用例。
5. 翻译 XML 三处同步（Keyed：标题/描述）。

### 新增动作分类

1. 在 `ActionGate` 新增 `private static readonly HashSet<string> XxxActions`。
2. 在 `BridgeRimChatSettings` 新增 `skipXxxActions` 字段（`ExposeData` + `ApplyDefaults` + UI + 翻译）。
3. 在 `ShouldSkipAction` 增加 `if (settings.skipXxxActions && XxxActions.Contains(intentId)) return true;` 分支。
4. 在 `ActionGateClassificationTests` 覆盖新分类的 intentId 命中/未命中、开关关闭、`forceRimMindActions` 短路。

### 新增 Context Pull Provider

1. 在 `ContextPullBridge.Register` 增加设置开关判断 + `RegisterXxxProvider` 私有方法。
2. 调用 `RimMindAPI.Context.ContextKeys.Register(new ContextProviderDef(...))`，`ownerMod` 必须传 `ModId`（`"RimMindBridgeRimChat"`）。
3. Pawn 作用域 provider 用 `TryFindPawnById(ctx.PawnId)` 解析 Pawn（已覆盖世界 pawns + 所有地图）。
4. manager 单例统一通过 `RimChatApiShim.TryGetManagerInstance(type)` 获取。
5. 在 `ContextPullBridgeTests` 补充注册/卸载用例；Pawn 查找用例归 `ContextPullBridgePawnLookupTests`。
6. 在 `Unregister` 增加对应 key 的卸载。
7. 翻译 XML 同步设置开关文案。

### 持久化扩展 (per-mod vs per-game)

- **per-mod 用户设置** → 加到 `BridgeRimChatSettings`（字段 + `ExposeData` + `ApplyDefaults` + UI + 翻译）。
- **per-game 运行时状态** → 加到 `SharedIncidentCooldown` 同级的静态类，`ExposeData` 标 `internal`，由 `GameComponent_BridgeRimChat.ExposeData` 委托调用。**禁止**把 per-game 状态塞进 `BridgeRimChatSettings`（会污染 ModSettings.xml 且无法跟随存档卸载）。
- 新增 per-game 状态后，在 `GameComponentBridgeRimChatTests` 补充 ExposeData 委托验证。

## 操作边界

### 必须做
- 所有对RimChat访问通过RimChatApiShim反射，反射调用包裹try-catch
- 新设置项在 `ExposeData` + `ApplyDefaults` + UI + 翻译XML 四处同步
- 注册Provider/SkipCheck/Listener用统一ModId `"RimMindBridgeRimChat"`（无点，匹配 About.xml packageId `mcocdaa.RimMindBridgeRimChat` 的后缀；由 `OwnerModIdConsistencyTests` 守护）
- RimChatApiShim.ResolveTypes 标记 NoInlining 防止类型加载异常
- per-game 运行时状态写入 `GameComponent_BridgeRimChat` + `SharedIncidentCooldown`（internal ExposeData），不要塞进 `BridgeRimChatSettings`
- manager 单例获取统一走 `RimChatApiShim.TryGetManagerInstance`，不要在各 Bridge 方法里重复 `GetStaticPropertyValue` + null 检查

### 先询问
- 修改动作门控分类逻辑(`DiplomacyActions`/`SocialActions`/`RecruitActions` 归属)
- 修改 `SharedIncidentCooldown` 冷却默认值(60000)
- 新增ContextPush方向(当前仅ContextPull)
- 修改RimChatApiShim中的类型名字符串(RimChat内部类型变更时)
- 新增 per-game 持久化字段（涉及存档兼容性）

### 绝对禁止
- 对RimChat编译期引用
- 反射访问RimChat `internal` 类型不包裹try-catch
- `forceRimMindActions` 开启时跳过Storyteller事件(两个门控独立)
- 设置变更时直接注册/卸载SkipCheck(通过委托实时读取)
- 直接调用 `SharedIncidentCooldown.ExposeData`（internal，仅 GameComponent 可调用）
- 把 per-game 状态写进 `BridgeRimChatSettings`（ModSettings 是 per-mod，不跟随存档）
- 在 `OwnerModId` 中使用点号或与 About.xml packageId 不一致的字符串
