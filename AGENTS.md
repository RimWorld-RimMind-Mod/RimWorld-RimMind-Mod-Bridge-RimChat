# AGENTS.md — RimMind-Bridge-RimChat

RimMind 与 RimChat 模组协调层，对话/动作门控 + 上下文拉取。

## 项目定位

纯反射(无编译期引用):
- **DialogueGate**: 注册SkipCheck防止与RimChat重复触发对话(Chitchat/Auto/PlayerInput三种triggerType独立门控)
- **ActionGate**: 注册ActionSkipCheck防止重复执行外交(adjust_faction/trigger_incident)、社交(romance_attempt/romance_breakup)、招募(recruit_agree)；注册IncidentSkipCheck+SharedIncidentCooldown防止叙事者事件重复
- **ContextPullBridge**: 通过RimChatApiShim反射读取RimChat外交/RPG对话历史，注册为RimMind ContextKey(rimchat_diplomacy/rimchat_rpg_history)
- **RimChatApiShim**: 反射封装层，统一处理RimChat类型解析和字段访问，延迟解析+NoInlining防止类型加载异常

## 构建

| 项 | 值 |
|----|-----|
| Target | net48, C#9.0, Nullable enable |
| Output | `../1.6/Assemblies/` |
| Assembly | RimMindBridgeRimChat |
| 依赖 | RimMindCore.dll, Krafs.Rimworld.Ref, Lib.Harmony.Ref |
| 无编译期引用 | RimChat(纯反射)，RimMind-Dialogue/Actions/Storyteller(运行时委托) |
| 测试 | Tests/ 目录，xunit，net10.0，仅测试RimChatApiShim |

## 源码结构

```
Source/
├── RimMindBridgeRimChatMod.cs        Mod入口
├── Bridge/
│   ├── DialogueGate.cs               对话门控(ShouldSkipDialogue/ShouldSkipFloatMenuOption)
│   ├── ActionGate.cs                 动作门控(ShouldSkipAction/ShouldSkipStorytellerIncident)
│   ├── ContextPullBridge.cs          上下文拉取(rimchat_diplomacy+rimchat_rpg_history)
│   └── RimChatApiShim.cs             反射封装(类型解析+字段访问)
├── Cooldown/SharedIncidentCooldown.cs 事件共享冷却
├── Detection/RimChatDetector.cs      检测RimChat激活+API可用性
└── Settings/BridgeRimChatSettings.cs  15项设置
Tests/
├── RimChatStubs.cs                   测试桩(AccessTools/Log/RimChatDetector)
└── RimChatApiShimTests.cs            RimChatApiShim单元测试
```

## 门控逻辑

```
ShouldSkipDialogue:
  Chitchat  → enableChitchatGate
  Auto      → enableAutoGate
  PlayerInput → enablePlayerInputGate && skipPlayerDialogue && !forceRimMindPlayerDialogue
  _         → false

ShouldSkipFloatMenuOption:
  enablePlayerInputGate && skipPlayerDialogue && !forceRimMindPlayerDialogue

ShouldSkipAction:
  enableActionGate && !forceRimMindActions &&
  (skipDiplomacyActions && intentId∈{adjust_faction,trigger_incident} ||
   skipSocialActions && intentId∈{romance_attempt,romance_breakup} ||
   skipRecruitAgree && intentId=="recruit_agree") → true

ShouldSkipStorytellerIncident:
  enableActionGate && skipTriggerIncident && SharedIncidentCooldown.IsOnCooldown → true
```

## Provider注册 (ModId: `"RimMind.BridgeRimChat"`)

| Category | 数据来源 | 类型 | 设置开关 |
|----------|---------|------|---------|
| rimchat_diplomacy | DiplomacyManager.dialogueSessions(反射) | Static | pullDiplomacyHistory |
| rimchat_rpg_history | RpgNpcDialogueArchiveManager._archiveCache(反射) | Pawn | pullRpgHistory |

使用 `ContextKeyRegistry.Register` 注册，provider 统一为 `Func<Pawn, List<ContextEntry>>`（静态上下文忽略 pawn 参数）。

## RimChatApiShim 反射封装

```
延迟解析: EnsureResolved() → ResolveTypes() [NoInlining]
  → AccessTools.TypeByName 解析3个类型:
    - RimChat.API.RimChatAPI
    - RimChat.DiplomacySystem.GameComponent_DiplomacyManager
    - RimChat.Memory.RpgNpcDialogueArchiveManager

工具方法:
  GetStaticPropertyValue(Type, propertyName) → object?
  GetInstanceFieldValue(instance, fieldName, BindingFlags) → object?
```

## 操作边界

### ✅ 必须做
- 所有对RimChat访问通过RimChatApiShim反射，反射调用包裹try-catch
- 新设置项在 `ExposeData` + UI + 翻译XML三处同步
- 注册Provider/SkipCheck用统一ModId
- RimChatApiShim.ResolveTypes 标记 NoInlining 防止类型加载异常

### ⚠️ 先询问
- 修改动作门控分类逻辑(`DiplomacyActions`/`SocialActions` 归属)
- 修改 `SharedIncidentCooldown` 冷却默认值(60000)
- 新增ContextPush方向(当前仅ContextPull)
- 修改RimChatApiShim中的类型名字符串(RimChat内部类型变更时)

### 🚫 绝对禁止
- 对RimChat编译期引用
- 反射访问RimChat `internal` 类型不包裹try-catch
- `forceRimMindActions` 开启时跳过Storyteller事件(两个门控独立)
- 设置变更时直接注册/卸载SkipCheck(通过委托实时读取)
