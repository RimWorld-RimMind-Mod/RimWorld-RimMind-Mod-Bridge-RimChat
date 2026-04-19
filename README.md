# RimMind - Bridge: RimChat

RimMind 套件与 RimChat 模组的协调层，当两个模组同时激活时实现对话/动作互斥与上下文拉取。

## RimMind 是什么

RimMind 是一套 AI 驱动的 RimWorld 模组套件，通过接入大语言模型（LLM），让殖民者拥有人格、记忆、对话和自主决策能力。

## 本模组的作用

RimChat 是另一个流行的 RimWorld AI 对话模组，具有外交系统和 RPG 对话功能。当 RimMind 和 RimChat 同时安装时，两个 AI 系统可能产生冲突（重复触发对话、重复执行动作、上下文不互通）。本模组作为协调层解决这些问题：

- **避免重复对话**：当 RimChat 激活时，自动跳过 RimMind 的玩家对话触发
- **避免重复动作**：跳过与 RimChat 功能重叠的 RimMind 动作（外交、社交、招募、事件触发）
- **数据互通**：从 RimChat 拉取外交/RPG 对话历史，注入 RimMind 上下文

## 子模组列表与依赖关系

| 模组 | 职责 | 依赖 | GitHub |
|------|------|------|--------|
| **RimMind-Core** | API 客户端、请求调度、上下文打包 | Harmony | [链接](https://github.com/mcocdaa/RimWorld-RimMind-Mod-Core) |
| RimMind-Actions | AI 控制小人的动作执行库 | Core | [链接](https://github.com/mcocdaa/RimWorld-RimMind-Mod-Actions) |
| RimMind-Advisor | AI 扮演小人做出工作决策 | Core, Actions | [链接](https://github.com/mcocdaa/RimWorld-RimMind-Mod-Advisor) |
| RimMind-Dialogue | AI 驱动的对话系统 | Core | [链接](https://github.com/mcocdaa/RimWorld-RimMind-Mod-Dialogue) |
| RimMind-Memory | 记忆采集与上下文注入 | Core | [链接](https://github.com/mcocdaa/RimWorld-RimMind-Mod-Memory) |
| RimMind-Personality | AI 生成人格与想法 | Core | [链接](https://github.com/mcocdaa/RimWorld-RimMind-Mod-Personality) |
| RimMind-Storyteller | AI 叙事者，智能选择事件 | Core | [链接](https://github.com/mcocdaa/RimWorld-RimMind-Mod-Storyteller) |
| **RimMind-Bridge-RimChat** | RimChat 协调层 | Core, RimChat(可选) | [链接](https://github.com/mcocdaa/RimWorld-RimMind-Mod-Bridge-RimChat) |

```
Core ── Actions ── Advisor
  ├── Dialogue
  ├── Memory
  ├── Personality
  ├── Storyteller
  └── Bridge-RimChat ←── RimChat (可选)
```

## 安装步骤

### 从源码安装

**Linux/macOS:**
```bash
git clone git@github.com:mcocdaa/RimWorld-RimMind-Mod-Bridge-RimChat.git
cd RimWorld-RimMind-Mod-Bridge-RimChat
./script/deploy-single.sh <your RimWorld path>
```

**Windows:**
```powershell
git clone git@github.com:mcocdaa/RimWorld-RimMind-Mod-Bridge-RimChat.git
cd RimWorld-RimMind-Mod-Bridge-RimChat
./script/deploy-single.ps1 <your RimWorld path>
```

### 从 Steam 安装

1. 安装 [Harmony](https://steamcommunity.com/sharedfiles/filedetails/?id=2009463077) 前置模组
2. 安装 RimMind-Core
3. 按需安装其他 RimMind 子模组
4. 安装 RimChat（可选，但推荐）
5. 安装 RimMind-Bridge-RimChat
6. 在模组管理器中确保加载顺序：Harmony → RimChat → Core → 其他子模组 → Bridge-RimChat

## 快速开始

本模组无需额外配置即可工作。安装后，当 RimChat 激活时自动启用桥接功能。

### 调整设置

1. 启动游戏，进入主菜单
2. 点击 **选项 → 模组设置 → RimMind-Core**
3. 切换到 **Bridge (RimChat)** 标签页
4. 按需调整对话门控、动作门控、上下文拉取选项

## 核心功能

### 对话门控

当 RimChat 激活时，自动跳过 RimMind-Dialogue 的玩家对话触发，避免两个 AI 对话系统冲突：

- **跳过玩家对话**：跳过 RimMind 的玩家对话触发并移除"与X对话"浮动菜单选项（默认开启）
- **保留 RimMind 玩家对话**：即使跳过玩家对话，仍保留 RimMind 的对话触发和浮动菜单选项，允许两套聊天系统共存（默认关闭）

### 动作门控

当 RimChat 激活时，自动跳过与 RimChat 功能重叠的 RimMind-Actions 动作和 RimMind-Storyteller 事件触发：

- **跳过外交类动作**：跳过 adjust_faction 和 trigger_incident 动作，由 RimChat 外交系统处理（默认开启）
- **跳过叙事者事件**：跳过 RimMind-Storyteller 的事件触发，避免与 RimChat 重复触发事件（默认开启）
- **跳过社交类动作**：跳过 romance_accept 和 romance_breakup 动作（默认关闭）
- **跳过招募动作**：跳过 recruit_agree 动作，由 RimChat 处理招募决策（默认关闭）
- **事件触发冷却**：两次叙事者事件触发之间的最短冷却时间（默认 1 游戏天，范围 0.1~3.0 天）
- **强制 RimMind 执行所有动作**：忽略互斥规则，RimMind-Actions 无视 RimChat 状态执行所有动作（不影响叙事者事件，默认关闭）

### 上下文拉取

从 RimChat 拉取对话历史，注册为 RimMind 的上下文 Provider，使 RimMind 的 AI 请求也能感知 RimChat 的对话内容：

- **拉取外交对话历史**：将 RimChat 的外交对话历史注册为 RimMind 的静态上下文 Provider（默认开启）
- **拉取 RPG 对话历史**：将 RimChat 的 RPG 对话历史注册为 RimMind 的 Pawn 上下文 Provider（默认关闭）

## 设置项

### 对话门控

| 设置 | 默认值 | 说明 |
|------|--------|------|
| 启用对话门控 | 开启 | RimChat 激活时跳过 RimMind 重复对话触发 |
| 跳过玩家对话 | 开启 | 跳过玩家对话触发并移除"与X对话"浮动菜单 |
| 保留 RimMind 玩家对话 | 关闭 | 跳过玩家对话时仍保留 RimMind 的对话触发和浮动菜单 |

### 动作门控

| 设置 | 默认值 | 说明 |
|------|--------|------|
| 启用动作门控 | 开启 | RimChat 激活时跳过重叠的 RimMind 动作和叙事者事件 |
| 跳过外交类动作 | 开启 | 跳过 adjust_faction 和 trigger_incident |
| 跳过叙事者事件 | 开启 | 跳过 RimMind-Storyteller 事件触发 |
| 跳过社交类动作 | 关闭 | 跳过 romance_accept 和 romance_breakup |
| 跳过招募动作 | 关闭 | 跳过 recruit_agree |
| 事件触发冷却 | 1 游戏天 | 两次叙事者事件触发的最短间隔（0.1~3.0 天） |
| 强制 RimMind 执行所有动作 | 关闭 | RimMind-Actions 忽略互斥规则执行所有动作（不影响叙事者事件） |

### 上下文拉取

| 设置 | 默认值 | 说明 |
|------|--------|------|
| 启用上下文拉取 | 开启 | 从 RimChat 拉取数据注册为 RimMind 上下文 Provider |
| 拉取外交对话历史 | 开启 | 将 RimChat 外交对话历史注入 RimMind 上下文 |
| 拉取 RPG 对话历史 | 关闭 | 将 RimChat RPG 对话历史注入 RimMind 上下文 |

## 常见问题

**Q: 不安装 RimChat 可以用吗？**
A: 可以。本模组通过反射检测 RimChat，未安装时所有桥接功能静默跳过，不会报错。

**Q: 只安装 RimChat 不安装其他 RimMind 子模组可以吗？**
A: 可以，但上下文拉取和动作门控功能需要对应的 RimMind 子模组才能生效。对话门控只需 RimMind-Dialogue。

**Q: 对话门控会不会导致完全没有对话？**
A: 不会。门控只是跳过 RimMind 的重复触发，RimChat 的对话系统正常工作。你可以通过"保留 RimMind 玩家对话"选项让两套系统共存。

**Q: "跳过外交类动作"和"跳过叙事者事件"有什么区别？**
A: "跳过外交类动作"跳过的是 RimMind-Actions 中的 adjust_faction 和 trigger_incident 动作ID；"跳过叙事者事件"跳过的是 RimMind-Storyteller 的事件触发，并支持冷却机制。两者互不干扰。

**Q: "强制 RimMind 执行所有动作"会影响叙事者事件吗？**
A: 不会。该选项仅影响 RimMind-Actions 的动作执行，不影响 RimMind-Storyteller 的事件触发。叙事者事件的跳过由"跳过叙事者事件"选项独立控制。

**Q: RimChat 更新后反射读取失败怎么办？**
A: 本模组所有反射读取都有 try-catch 保护。RimChat 内部结构变更只会导致对应功能静默失效，不会崩溃。

## 致谢

本项目开发过程中参考了以下优秀的 RimWorld 模组：

- [RimChat](https://github.com/YancyGao/RimChat) - 对话系统与外交系统参考

## 贡献

欢迎提交 Issue 和 Pull Request！如果你有任何建议或发现 Bug，请通过 GitHub Issues 反馈。


---

# RimMind - Bridge: RimChat (English)

The coordination layer between the RimMind suite and RimChat mod, enabling dialogue/action mutual exclusion and context pulling when both mods are active.

## What is RimMind

RimMind is an AI-driven RimWorld mod suite that connects to Large Language Models (LLMs), giving colonists personality, memory, dialogue, and autonomous decision-making.

## What This Mod Does

RimChat is another popular RimWorld AI dialogue mod with diplomacy and RPG dialogue features. When both RimMind and RimChat are installed, the two AI systems may conflict (duplicate dialogue triggers, duplicate action execution, no context sharing). This mod serves as a coordination layer to resolve these issues:

- **Prevent duplicate dialogues**: Automatically skip RimMind's player dialogue triggers when RimChat is active
- **Prevent duplicate actions**: Skip RimMind actions that overlap with RimChat's capabilities (diplomacy, social, recruitment, incident triggers)
- **Data sharing**: Pull RimChat's diplomacy/RPG dialogue history and inject into RimMind's context

## Sub-Modules & Dependencies

| Module | Role | Depends On | GitHub |
|--------|------|------------|--------|
| **RimMind-Core** | API client, request dispatch, context packaging | Harmony | [Link](https://github.com/mcocdaa/RimWorld-RimMind-Mod-Core) |
| RimMind-Actions | AI-controlled pawn action execution | Core | [Link](https://github.com/mcocdaa/RimWorld-RimMind-Mod-Actions) |
| RimMind-Advisor | AI role-plays colonists for work decisions | Core, Actions | [Link](https://github.com/mcocdaa/RimWorld-RimMind-Mod-Advisor) |
| RimMind-Dialogue | AI-driven dialogue system | Core | [Link](https://github.com/mcocdaa/RimWorld-RimMind-Mod-Dialogue) |
| RimMind-Memory | Memory collection & context injection | Core | [Link](https://github.com/mcocdaa/RimWorld-RimMind-Mod-Memory) |
| RimMind-Personality | AI-generated personality & thoughts | Core | [Link](https://github.com/mcocdaa/RimWorld-RimMind-Mod-Personality) |
| RimMind-Storyteller | AI storyteller, smart event selection | Core | [Link](https://github.com/mcocdaa/RimWorld-RimMind-Mod-Storyteller) |
| **RimMind-Bridge-RimChat** | RimChat coordination layer | Core, RimChat (optional) | [Link](https://github.com/mcocdaa/RimWorld-RimMind-Mod-Bridge-RimChat) |

## Installation

### Install from Source

**Linux/macOS:**
```bash
git clone git@github.com:mcocdaa/RimWorld-RimMind-Mod-Bridge-RimChat.git
cd RimWorld-RimMind-Mod-Bridge-RimChat
./script/deploy-single.sh <your RimWorld path>
```

**Windows:**
```powershell
git clone git@github.com:mcocdaa/RimWorld-RimMind-Mod-Bridge-RimChat.git
cd RimWorld-RimMind-Mod-Bridge-RimChat
./script/deploy-single.ps1 <your RimWorld path>
```

### Install from Steam

1. Install [Harmony](https://steamcommunity.com/sharedfiles/filedetails/?id=2009463077)
2. Install RimMind-Core
3. Install other RimMind sub-modules as needed
4. Install RimChat (optional, but recommended)
5. Install RimMind-Bridge-RimChat
6. Ensure load order: Harmony → RimChat → Core → other sub-modules → Bridge-RimChat

## Quick Start

This mod works out of the box with no additional configuration. After installation, bridge features are automatically enabled when RimChat is active.

### Adjust Settings

1. Launch the game, go to main menu
2. Click **Options → Mod Settings → RimMind-Core**
3. Switch to the **Bridge (RimChat)** tab
4. Adjust dialogue gate, action gate, and context pull options as needed

## Key Features

### Dialogue Gate

When RimChat is active, automatically skips RimMind-Dialogue's player dialogue triggers to prevent conflicts:

- **Skip player dialogue**: Skip RimMind's player dialogue triggers and remove the "Chat with X" float menu option (default: on)
- **Keep RimMind player dialogue**: Keep RimMind's dialogue triggers and float menu option even when skipping, allowing both chat systems to coexist (default: off)

### Action Gate

When RimChat is active, automatically skips RimMind-Actions that overlap with RimChat's capabilities and RimMind-Storyteller incident triggers:

- **Skip diplomacy actions**: Skip adjust_faction and trigger_incident actions, handled by RimChat's diplomacy system (default: on)
- **Skip Storyteller incident**: Skip RimMind-Storyteller incident triggers to avoid duplicate event firing with RimChat (default: on)
- **Skip social actions**: Skip romance_accept and romance_breakup actions (default: off)
- **Skip recruit action**: Skip recruit_agree action, let RimChat handle recruitment decisions (default: off)
- **Incident cooldown**: Minimum cooldown between two Storyteller incident triggers (default: 1 game day, range 0.1~3.0 days)
- **Force RimMind to execute all actions**: Ignore mutual exclusion rules, RimMind-Actions executes all actions regardless of RimChat status (does not affect Storyteller incidents, default: off)

### Context Pull

Pulls RimChat's dialogue history and registers as RimMind context providers, enabling RimMind's AI requests to perceive RimChat's conversation content:

- **Pull diplomacy dialogue history**: Register RimChat's diplomacy dialogue history as RimMind's static context provider (default: on)
- **Pull RPG dialogue history**: Register RimChat's RPG dialogue history as RimMind's pawn context provider (default: off)

## Settings

### Dialogue Gate

| Setting | Default | Description |
|---------|---------|-------------|
| Enable dialogue gate | On | Skip RimMind redundant triggers when RimChat is active |
| Skip player dialogue | On | Skip player dialogue triggers and remove "Chat with X" float menu |
| Keep RimMind player dialogue | Off | Keep RimMind triggers and float menu when skipping player dialogue |

### Action Gate

| Setting | Default | Description |
|---------|---------|-------------|
| Enable action gate | On | Skip overlapping RimMind actions and Storyteller incidents when RimChat is active |
| Skip diplomacy actions | On | Skip adjust_faction and trigger_incident |
| Skip Storyteller incident | On | Skip RimMind-Storyteller incident triggers |
| Skip social actions | Off | Skip romance_accept and romance_breakup |
| Skip recruit action | Off | Skip recruit_agree |
| Incident cooldown | 1 game day | Minimum interval between Storyteller incident triggers (0.1~3.0 days) |
| Force RimMind to execute all actions | Off | RimMind-Actions ignores mutual exclusion rules (does not affect Storyteller incidents) |

### Context Pull

| Setting | Default | Description |
|---------|---------|-------------|
| Enable context pull | On | Pull data from RimChat and register as RimMind context providers |
| Pull diplomacy dialogue history | On | Inject RimChat diplomacy dialogue history into RimMind context |
| Pull RPG dialogue history | Off | Inject RimChat RPG dialogue history into RimMind context |

## FAQ

**Q: Can I use this without RimChat?**
A: Yes. This mod detects RimChat via reflection and silently skips all bridge features when RimChat is not installed.

**Q: Can I use this with only RimChat and no other RimMind sub-modules?**
A: Yes, but context pull and action gate features require the corresponding RimMind sub-modules to be effective. Dialogue gate only requires RimMind-Dialogue.

**Q: Will dialogue gate cause no dialogues at all?**
A: No. The gate only skips RimMind's redundant triggers; RimChat's dialogue system works normally. You can use the "Keep RimMind player dialogue" option to allow both systems to coexist.

**Q: What's the difference between "Skip diplomacy actions" and "Skip Storyteller incident"?**
A: "Skip diplomacy actions" skips the adjust_faction and trigger_incident action IDs from RimMind-Actions. "Skip Storyteller incident" skips RimMind-Storyteller's incident triggers with a cooldown mechanism. They are independent of each other.

**Q: Does "Force RimMind to execute all actions" affect Storyteller incidents?**
A: No. This option only affects RimMind-Actions execution and does not affect RimMind-Storyteller incident triggers. Storyteller incident skipping is independently controlled by the "Skip Storyteller incident" option.

**Q: What if RimChat's internal structure changes after an update?**
A: All reflection reads have try-catch protection. Internal structure changes will only cause affected features to silently fail without crashing.

## Acknowledgments

This project references the following excellent RimWorld mods:

- [RimChat](https://github.com/YancyGao/RimChat) - Dialogue system & diplomacy system reference

## Contributing

Issues and Pull Requests are welcome! If you have any suggestions or find bugs, please feedback via GitHub Issues.
