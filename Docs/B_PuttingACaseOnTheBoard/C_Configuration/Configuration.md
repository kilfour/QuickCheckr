# Configuration
[The Board][PuttingACaseOnTheBoard] | [Previous][RunningACheckr] | [Next][TheEvidenceVault]  
  
QuickCheckr runs are configured through an immutable `CheckrConfig` record. 
You typically start from `CheckrConfig.Default()` and then tweak the parts you care about.

All configuration options live in the `QuickCheckr.Protocol` namespace.  

**Default Configuration:**  
```csharp
{
    FileAs = string.Empty,
    Custodian = Custodian.Default,
    ShrinkMode = ShrinkMode.Default,
    ReportMode = ReportMode.Default,
    DeliberationPolicy = null,
    DeliberationTarget = null,
    InternalAffairs = []
}
```
## Filing Evidence
QuickCheckr can be configured to persist the results of a test run.  
```csharp
private static CheckrConfig Configure(CheckrConfig config)
    => config with { FileAs = "MyQuickCheckrTest" };
```
Above configuration will write the resulting report of a run to a `report.qr` file in a folder relative to the solution root:  
```csharp
.quickcheckr\MyQuickCheckrTest\report.qr
```
In case a test passes, a *summary report* is provided.  
```text
2 Runs
 Passed Expectations
 - Always Passes: 10x
 ------------------------------------------------------------
```
In addition a sub folder is created containing the evidence vault files:  
```csharp
.quickcheckr\MyQuickCheckrTest\vault\evidence.qv
```
```csharp
.quickcheckr\MyQuickCheckrTest\vault\evidence-log.ql
```

    - `evidence.qv` contains a list of all failing seeds, whenever a test fails, the seed is added to this file.
    - `evidence-log.ql` is an append only log of all evidence vault actions.

    This *evidence* can later be used to potentially solve *Cold Cases*.  
## The Custodian
`TheCustodian` is in charge of storing and retrieving QuickCheckr artifacts.  
As we saw in 'Filing Evidence', QuickCheckr stores reports and failing seeds on disk.  
This is the default behaviour, but by configuring the `Custodian` we can change this behaviour.

Currently only one extra `Custodian` is provided: `Custodian.Local()`.  
This one stores everything in a '.quickcheckr' directory in the same directory as the test file that created it.
  
### Custom Custodians
You can also supply your own implementation:  
```csharp
namespace QuickCheckr.Tests.Docs.B_PuttingACaseOnTheBoard.C_Configuration.B_TheCustodian;

public class InMemoryCustodian(HashSet<VaultEntry> StartingEvidence) : ICustodian
{
    public Dictionary<string, CaseFile> CaseFiles { get; private set; } = [];

    public void StoreCaseFile(string path, CaseFile caseFile, Flow<CaseFile> styleGuide)
        => CaseFiles[path] = caseFile;

    public string RemoveFileCalledWith { get; private set; } = string.Empty;
    public void RemoveCaseFile(string path)
    {
        RemoveFileCalledWith = path;
        CaseFiles.Remove(path);
    }

    public string EvidenceFileName { get; private set; } = string.Empty;
    public HashSet<VaultEntry> Entries { get; private set; } = StartingEvidence;
    public void StoreVaultEntries(string path, IEnumerable<VaultEntry> entries)
    {
        EvidenceFileName = path;
        Entries = [.. entries];
    }

    public HashSet<VaultEntry> RetrieveVaultEntries(string path)
        => Entries;

    public string LogFileName { get; private set; } = string.Empty;
    public List<string> Log = [];
    public void LogVaultAction(string path, string line)
    {
        LogFileName = path;
        Log.Add(line);
    }
}
```
This one stores evidence and reports in memory, and is used in the tests to avoid touching the filesystem.

Obviously, this is the hook provided for when you would like to produce your own artifacts in a CI build.  
## Deliberation Policy
Sometimes the shrinker comes up with a *local* minimal fail-case, not *the* minimal fail-case.  
Configuring a *DeliberationPolicy* allows us to instruct the Shrinker to explore the input further in a directed fasion.  

In short: Deliberation controls how agressive the shrinker keeps searching after it has found a *local* minimal fail-case.

Consider the following model:  
```csharp
public class Account
{
    public int Balance { get; private set; } = 0;
    public void Deposit(int amount)
    {
        Balance += amount;
        ApplyFee();
    }
    public void WithDraw(int amount)
    {
        if (amount > Balance) return;
        Balance -= amount;
        ApplyFee();
    }
    private void ApplyFee() => Balance -= 1;
}
```
It's a simple bank account with a no-overdraft invariant and a fee applied on each transaction.  
We can verify this with the following Checkr:  
```csharp
from account in Trackr.Stashed(() => new Account())
from amount in Checkr.Input("Amount", Fuzzr.Int(1, 10))
from operations in Checkr.OneOf(
    Checkr.Act("Deposit", () => account.Deposit(amount)),
    Checkr.Act("Withdraw", () => account.WithDraw(amount))
)
from noOverdraft in Checkr.Expect("No Overdraft", () => account.Balance >= 0)
select Case.Closed;
```
Now if we run this we might get something like this:  
```text
------------------------------------------------------------
 Test:                    RunTest
 Location:                Docs\B_PuttingACaseOnTheBoard\C_Configuration\F_DeliberationPolicy\DeliberationPolicy.cs:66:1
 Original failing run:    3 executions
 Seed:                    1920827676
 ------------------------------------------------------------
  Executed: Deposit
   - Input: Amount = 8
 ------------------------------------------------------------
  Executed: Withdraw
   - Input: Amount = 1
 ------------------------------------------------------------
  Executed: Withdraw
   - Input: Amount = 5
 =======================================
  !! Expectation Failed: No Overdraft
 =======================================
 Passed Expectations
 - No Overdraft: 2x
 ------------------------------------------------------------
```
As you can see, this is a *local* minimal fail-case, but a smaller one exists.  

Let's add a *DeliberationPolicy*, looking for a fail-case with less executions:  
```csharp
private static CheckrConfig Configure(CheckrConfig config)
    => config with { DeliberationPolicy = DeliberationPolicy.MinimumExecutions };
```
Running this test now, with `100.Runs()` and `10.ExecutionsPerRun()` consistently produces a two execution failure.  
```text
------------------------------------------------------------
 Test:                    RunTest_Deliberated
 Location:                Docs\B_PuttingACaseOnTheBoard\C_Configuration\F_DeliberationPolicy\DeliberationPolicyTests.cs:113:1
 Original failing run:    7 executions
 Minimal failing case:    2 executions (after 5 shrinks)
 Seed:                    136908528
 ------------------------------------------------------------
  Executed: Deposit
   - Input: Amount = 3
 ------------------------------------------------------------
  Executed: Withdraw
   - Input: Amount = 2
 =======================================
  !! Expectation Failed: No Overdraft
 =======================================
 Passed Expectations
 - No Overdraft: 6x
 ------------------------------------------------------------
```
The following predefined policies are available  
- `DeliberationPolicy.MinimumExecutions`  
- `DeliberationPolicy.MinimumOriginalExecutions`  
- `DeliberationPolicy.MinimumShrinkCount`  
- `DeliberationPolicy.MaximumShrinkCount`  
- `DeliberationPolicy.MinimumWarnings`  
- `DeliberationPolicy.MaximumExecutionsShrunk`  
- `DeliberationPolicy.MaximumDistinctPassedExpectations`  
## ReportMode
You can use this to customize report formatting somewhat.  
I personally almost always disable the *Labels* ('Input:', 'Pool:', etc...), but your milage may vary.  

It is also useful to toggle traces on or off, for instance.  
```csharp
private static CheckrConfig DisableTraces(CheckrConfig config) =>
    config with { ReportMode = config.ReportMode & ~ReportMode.Trace };
```
A full reference to the available options can be found [here][A_ReportModeReference].  
## ShrinkMode
Controls which shrinking strategies are active.  

This is an option you will likely never need.  
It's main use is to investigate what happened during shrinking.  
As such, disabling shrinking entirely is probably the most useful step.  
```csharp
private static CheckrConfig DisableShrinking(CheckrConfig config) =>
    config with { ShrinkMode = ShrinkMode.None };
```
A full reference to the available options can be found [here][A_ShrinkModeReference].  
## Hint
If you do not like the defaults,
it is advised to simply wrap them into your own `CheckrOf` extension method(s):  
  
**Fast Deliberation:**  
```csharp
public static CaseFile Deliberate(
    this CheckrOf<Case> checkr,
    Func<ICaseFileSummary, int>? policy = null)
    => checkr.Run(50.Runs(), 20.ExecutionsPerRun(),
        cfg => cfg with { DeliberationPolicy = policy ?? DeliberationPolicy.MinimumExecutions });
```
**Auto File:**  
```csharp
public static CaseFile FileThis(
    this CheckrOf<Case> checkr,
    [CallerFilePath] string filePath = "",
    [CallerMemberName] string testName = "")
    => checkr.Run(50.Runs(), 20.ExecutionsPerRun(),
         cfg => cfg with { FileAs = $"{Path.GetFileName(filePath)}-{testName}" });
```
**Investigating:**  
```csharp
public static void Investigate(
    this CheckrOf<Case> checkr,
    [CallerFilePath] string filePath = "",
    [CallerMemberName] string testName = "")
    => checkr.GatherEvidence(100.Investigations(), 10.Runs(), 20.ExecutionsPerRun(),
        cfg => cfg with { FileAs = $"{Path.GetFileName(filePath)}-{testName}" });
```
You get the idea.  

[PuttingACaseOnTheBoard]: ../PuttingACaseOnTheBoard.md

[RunningACheckr]: ../B_RunningACheckr/RunningACheckr.md

[TheEvidenceVault]: ../D_TheEvidenceVault/TheEvidenceVault.md

[A_ReportModeReference]: J_ReportMode/Reference/A_ReportModeReference.md

[A_ShrinkModeReference]: N_ShrinkMode/Reference/A_ShrinkModeReference.md
