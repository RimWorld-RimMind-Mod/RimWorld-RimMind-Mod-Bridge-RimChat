<div align="center">

# RimMind-Bridge-RimChat 🌉
### Mutual Exclusion Gating & History Pull Bridge for RimWorld 1.6 & 1.7

**English** | [简体中文](README_zh.md)

<p>
  <a href="https://rimworldgame.com/"><img src="https://img.shields.io/badge/RimWorld-1.6%20%7C%201.7-brightgreen.svg" alt="RimWorld 1.6 | 1.7"></a>
  <a href="https://github.com/RimWorld-RimMind-Mod/RimWorld-RimMind-Mod-Core"><img src="https://img.shields.io/badge/Dependency-RimMind--Core-blue.svg" alt="Dependency: RimMind-Core"></a>
  <a href="#"><img src="https://img.shields.io/badge/Unit%20Tests-8%20Contracts%20Passing-success.svg" alt="Unit Tests"></a>
  <a href="LICENSE"><img src="https://img.shields.io/badge/License-MIT-yellow.svg" alt="License: MIT"></a>
</p>

<p><em>Enables seamless coexistence with RimChat through dialogue/action gating, shared storyteller cooldowns, and one-way context pull.</em></p>

</div>

---

## 📖 Overview

**RimMind-Bridge-RimChat** provides configurable mutual-exclusion gating, shared storyteller cooldown coordination, and one-way history pull when playing with both **RimMind** and the third-party **RimChat** mod. It prevents duplicate chat windows and overlapping diplomacy/social/recruitment actions, and imports recent RimChat diplomacy and RPG logs into RimMind's context engine.

### Key Capabilities
- **Dialogue Gating**: Independently gates RimMind's chitchat, auto dialogue, player input, and "Chat with X" float-menu triggers when RimChat is active, with an optional override to keep both player chat entries.
- **Action & Storyteller Gating**: Classifies diplomacy, social/romance, and recruitment tools via `ToolManifest` metadata (with explicit fallbacks) and coordinates storyteller incident triggers through a save-persisted shared cooldown (default 60,000 ticks / 1 in-game day).
- **One-Way Context Pull (RimChat → RimMind)**: Safely reads RimChat's diplomacy sessions (`rimchat_diplomacy`) and optional per-colonist RPG archive (`rimchat_rpg_history`) via main-thread snapshots and cached reflection, automatically circuit-breaking on schema mismatch.

---

## 🎮 In-Game Showcase

![RimMind-Bridge-RimChat Showcase](docs/images/showcase.jpg)
*Harmonious coexistence in action: RimMind and RimChat active in the same colony without duplicate dialogue triggers or conflicting diplomacy actions.*

---

## 🏛️ Gating & Context Pull Architecture

```mermaid
flowchart TD
    Event["Dialogue / Action / Incident Trigger"] --> Bridge["RimMind-Bridge-RimChat"]
    Bridge --> Check{"RimChat Active & Gate Enabled?"}
    Check -- Yes --> Suppress["Suppress RimMind Trigger & Defer to RimChat"]
    Check -- No --> Allow["Allow RimMind Pipeline"]
    RimChatLogs["RimChat Diplomacy & RPG Archives"] --> Pull["ContextPullBridge (Main-Thread Snapshot)"]
    Pull --> CoreContext["RimMind-Core ContextKeys (rimchat_diplomacy / rimchat_rpg_history)"]
```

---

## 🛠️ Installation & Load Order

```text
1. Harmony
2. Core (Vanilla RimWorld)
3. RimChat (Required third-party mod)
4. RimMind-Core
5. RimMind-Bridge-RimChat
```

---

## 🧪 Developer Guide & Testing

Run unit tests and dual-version builds from the workspace root:

```powershell
dotnet test RimMind-Bridge-RimChat/Tests/RimMindBridgeRimChat.Tests.csproj -c Release
dotnet build RimMind-Bridge-RimChat/Source/RimMindBridgeRimChat.csproj -c Release -p:GameVersion=1.6
dotnet build RimMind-Bridge-RimChat/Source/RimMindBridgeRimChat.csproj -c Release -p:GameVersion=1.7
```

---

## 📜 License

Licensed under the [MIT License](LICENSE).
