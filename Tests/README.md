# RimMind Bridge RimChat compact contracts

Active contract sources are `Contracts/*.cs`. The compact suite contains:

- `RimChatGateContracts` — dialogue/action classifications, float-menu parity and shared incident cooldown.
- `RimChatContextApiContracts` — provider registration and execution, bounded
  diplomacy projection, cross-map pawn lookup and reflection API behavior.
- `RimChatCompatibilityContracts` — safe defaults, absent dependency behavior and extension ownership.

Expected discovery after cutover: 8 Facts, below the Bridge-RimChat target of 36.

## Cutover handoff

- Active include: `Contracts/**/*.cs`
- Shared support include:

  ```xml
  <Compile Include="..\..\RimMind-Core\TestSupport\ContractCaseRunner.cs"
           Link="Support\ContractCaseRunner.cs" />
  ```

- Required retained stub include: `RimChatStubs.cs` for Verse/Core registries
  and UI adapters only.
- Required production includes: `RimChatApiShim.cs`, `DialogueGate.cs`,
  `ActionGate.cs`, `ContextPullBridge.cs`, `RimChatDetector.cs`,
  `BridgeRimChatSettings.State.cs`, `SharedIncidentCooldown.cs`,
  `GameComponent_BridgeRimChat.cs`, all five skip/listener extension sources,
  `RimChatSettingsTab.cs`, and `RimMindBridgeRimChatMod.cs`
- Legacy compile categories to remove from the project entry during cutover:
  dialogue/action gate matrices, action classification source-shape checks,
  cooldown/component matrices, context registration/pawn lookup matrices,
  reflection shim matrices, settings/default matrices, detector matrices,
  extension metadata matrices and owner-id duplication checks.

The active suite compiles the production settings state and detector. Stable
internal seams provide dependency probes, reflected type bindings, Pawn lookup
and cooldown reset without binding tests to private field names. The registered
diplomacy provider is executed rather than inferred from registration metadata.

## Retired legacy tests

Files outside `Contracts/` are retained on disk but excluded from compilation.
Their behavior mapping is recorded in the root contract mapping document.
Deletion requires explicit owner approval for each exact file path; directories are never deleted.
