# RimMind-Bridge-RimChat Tests

Active contract suites live in `Contracts/*.cs` (8 xUnit Facts total):

- `RimChatGateContracts` — dialogue/action classifications (`ToolManifest` + explicit fallbacks), float-menu parity, shared incident cooldown, per-game isolation, and save/load persistence.
- `RimChatContextApiContracts` — coordinator & context-pull registration, bounded diplomacy projection (matching RimChat's static-field `Instance` + private `dialogueSessions`), cross-map active-colonist lookup, and reflection shim behavior.
- `RimChatCompatibilityContracts` — safe defaults, absent-dependency behavior, and extension ownership.

`RimChatStubs.cs` supplies only external Verse/Core/UI seams needed to compile and exercise production bridge sources directly on `net10.0` (`LangVersion 9.0`).

## Run

```powershell
dotnet test RimMind-Bridge-RimChat/Tests/RimMindBridgeRimChat.Tests.csproj -c Release
```
