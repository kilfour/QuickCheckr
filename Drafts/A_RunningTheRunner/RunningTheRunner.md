# Running the Runner
QuickCheckr is fully **framework agnostic**: it doesn't care whether you run your tests with xUnit, NUnit, MSTest, or TUnit. There are no base classes to inherit from, no runner integrations to install, and no magic attributes controlling behaviour.  

A Checkr is just a LINQ expression that you evaluate by calling `Run()`.  

If a minimal failing case is found, QuickCheckr reports it by **throwing a `FalsifiableException`**, which contains the complete shrunk report, including the seed, execution steps, inputs, actions, traces, and the reason for failure.  

Because the failure is surfaced as a standard exception, any test framework can treat it as a normal assertion failure and you get a rich, human-readable report for free.  
## Configuring the Runner
QuickCheckr runs are configured through an immutable `CheckrConfig` record. You typically start from `CheckrConfig.Default()` and then tweak the parts you care about:

```csharp
check.Run(10.Runs(), 5.ExecutionsPerRun(), cfg => cfg with
{
    Seed = 123456,
    ShrinkMode = ShrinkMode.Default,
    Investigations = Investigate.All,
    FileAs = "LockTheStableDoors",
    EvidenceMode = EvidenceMode.ReviewAndPurge
});
```

Each property controls a different aspect of how the engine behaves:

**`Seed`:**

An optional fixed seed for reproducible runs.

* `null` (default) => a new random seed is chosen for each run.
* Any `int` => the full run (including shrinking) becomes deterministic.

Use this when you want to reproduce a particular failure locally or on CI.

**`Investigations`:**

An array of enabled investigation modes (for example `Investigate.Events`, `Investigate.InputShrinking`, `Investigate.NullIssues`, …).

* Leave empty (`[]`) for a quiet run.
* Use `Investigate.All` while you are still designing a Checkr or debugging a tricky issue.

**`ShrinkMode`:**

Controls which shrinking strategies are active:

* `ShrinkMode.None` => no shrinking; QuickCheckr reports the original failing run.
* `ShrinkMode.Executions` => trims away irrelevant executions.
* `ShrinkMode.Inputs` => trims and rewrites inputs to find a minimal failing case.
* `ShrinkMode.Reduction` => applies configured reducers for fine‑grained shrinking.
* `ShrinkMode.Default` => the recommended combination for day‑to‑day use.

You can also combine flags explicitly, e.g. `ShrinkMode.Executions | ShrinkMode.Inputs`.

**`FileAs`:**
An optional label that tells QuickCheckr to persist case files under a `.quickcheckr/{FileAs}` folder relative to the solution root.
* `""` (empty, default) => nothing is written to disk.
* Any non‑empty string => 
  * Failing runs are transcribed to `report.qr`.
  * The seed of any failing run is stored in the **evidence vault** (`evidence.qv`).

**`EvidenceMode`:**
Controls how QuickCheckr interacts with the **evidence vault**, a small on‑disk store of failing seeds per test (`.quickcheckr/{FileAs}/evidence.qv`).

* `EvidenceMode.Ignore` (default)
  * No interaction with the vault. Failing seeds are not replayed.
* `EvidenceMode.Review`
  * Before running fresh cases, QuickCheckr replays all stored seeds.
  * For each seed it logs whether the run still fails or now passes.
* `EvidenceMode.ReviewAndPurge`
  * Same as `Review`, but any seed that now **passes** is automatically removed from the vault.

Whenever a new failing case is found, its `(seed, executionsPerRun)` pair is automatically added to the vault for the current `FileAs`. Over time this gives you a lightweight regression safety net: once a bug has been caught, its failure seed stays on record until you deliberately purge it by running with `ReviewAndPurge`.
  
