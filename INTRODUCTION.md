# RimMind - Bridge: RimChat

RimMind 套件与 RimChat 模组的协调层，当两个模组同时激活时实现对话/动作互斥与上下文拉取。

## 核心能力

**对话门控** - 当 RimChat 激活时，自动跳过 RimMind-Dialogue 的玩家对话触发和浮动菜单，避免两个 AI 对话系统冲突。

**动作门控** - 当 RimChat 激活时，自动跳过与 RimChat 功能重叠的 RimMind-Actions 动作（外交、社交、招募）和 RimMind-Storyteller 事件触发，避免重复执行。支持共享冷却，防止短时间内重复触发事件。

**上下文拉取** - 从 RimChat 拉取外交对话历史和 RPG 对话历史，注册为 RimMind 的上下文 Provider，使 RimMind 的 AI 请求也能感知 RimChat 的对话内容。

## 技术亮点

- 纯反射读取 RimChat 数据，无编译期依赖，RimChat 未安装时静默跳过
- 三层门控：对话门控 + 动作门控 + 叙事者事件冷却
- 13 个可配置选项，精确控制门控范围和拉取内容

## 建议配图

1. 设置界面截图（展示对话门控、动作门控、上下文拉取三个分区）
2. RimChat 外交对话截图（展示 RimMind 上下文拉取效果）

---

# RimMind - Bridge: RimChat (English)

The coordination layer between the RimMind suite and RimChat mod, enabling dialogue/action mutual exclusion and context pulling when both mods are active.

## Key Features

**Dialogue Gate** - When RimChat is active, automatically skips RimMind-Dialogue's player dialogue triggers and float menu options, preventing conflicts between two AI dialogue systems.

**Action Gate** - When RimChat is active, automatically skips RimMind-Actions that overlap with RimChat's capabilities (diplomacy, social, recruitment) and RimMind-Storyteller incident triggers, avoiding duplicate execution. Supports shared cooldown to prevent repeated event triggers in a short time.

**Context Pull** - Pulls RimChat's diplomacy dialogue history and RPG dialogue history, registering them as RimMind context providers so RimMind's AI requests can also perceive RimChat's conversation content.

## Technical Highlights

- Pure reflection-based RimChat data reading with no compile-time dependency; silently skips when RimChat is not installed
- Three-layer gating: dialogue gate + action gate + storyteller incident cooldown
- 13 configurable options for precise gate scope and pull content control

## Suggested Screenshots

1. Settings interface (showing Dialogue Gate, Action Gate, Context Pull sections)
2. RimChat diplomacy dialogue (showing RimMind context pull effect)
