<div align="center">

# RimMind-Bridge-RimChat 🌉
### Seamless Compatibility Bridge & Mutual Exclusion Gating for RimWorld 1.6

**English** | [简体中文](README_zh.md)

<p>
  <a href="https://rimworldgame.com/"><img src="https://img.shields.io/badge/RimWorld-1.6-brightgreen.svg" alt="RimWorld 1.6"></a>
  <a href="https://github.com/mcocdaa/RimWorld-RimMind-Mod-Core"><img src="https://img.shields.io/badge/Dependency-RimMind--Core-blue.svg" alt="Dependency: RimMind-Core"></a>
  <a href="#"><img src="https://img.shields.io/badge/Unit%20Tests-10%2B%20Passing-success.svg" alt="Unit Tests"></a>
  <a href="LICENSE"><img src="https://img.shields.io/badge/License-MIT-yellow.svg" alt="License: MIT"></a>
</p>

<p><em>Enables seamless coexistence with the RimChat mod without conflicting dialogue bubbles or duplicated actions.</em></p>

</div>

---

## 📖 Overview

**RimMind-Bridge-RimChat** provides intelligent mutual exclusion gating and state synchronization when playing with both **RimMind** and the third-party **RimChat** mod simultaneously. It prevents overlapping overhead dialogue spam, deconflicts job dispatches, and shares interaction cooldowns.

### Key Capabilities
- **Dialogue Gating**: Automatically detects RimChat's conversational active window and suppresses duplicate RimMind dialogue triggers on the same colonist.
- **Action Deconfliction**: Ensures composite mechanism actions and RimChat job orders never collide on target pawns.
- **Context Synchronization**: Safely extracts recent RimChat diplomatic dialogues into RimMind's cognitive memory layers.

---

## 🎮 In-Game Showcase

![RimMind-Bridge-RimChat Showcase](docs/images/showcase.jpg)
*Harmonious coexistence in action: Both RimMind and RimChat active in the same colony without overlapping dialogue bubbles or job conflicts.*

---

## 🏛️ Gating Architecture

```mermaid
flowchart TD
    Event["Social / Dialogue Trigger"] --> Bridge["RimMind-Bridge-RimChat"]
    Bridge --> Check{"Is RimChat Currently Active?"}
    Check -- Yes --> Suppress["Suppress RimMind Trigger & Yield"]
    Check -- No --> Allow["Forward to RimMind-Dialogue Pipeline"]
    Bridge --> Sync["Bidirectional Context Synchronization"]
```

---

## 🛠️ Installation & Load Order

```text
1. Harmony
2. Core (Vanilla RimWorld)
3. RimChat (Optional third-party mod)
4. RimMind-Core
5. RimMind-Bridge-RimChat
```

---

## 🧪 Developer Guide & Testing

Run unit tests directly:

```powershell
dotnet test RimMind-Bridge-RimChat/Tests/RimMindBridgeRimChat.Tests.csproj -c Release
```

---

## 📜 License

Licensed under the [MIT License](LICENSE).
