# Configuration
[Up][PuttingACaseOnTheBoard] | [Previous][RunningACheckr] | [Next][TheEvidenceVault]  
  
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

!! WARNING: **Experimental Feature** !!

> This feature is **experimental**.  
> Its API, behaviour, or semantics may change substantially in future versions.  
>  
> Use with caution in production code.  
> It is included for users who want to experiment and help shape the future of QuickCheckr.

  
You can also supply your own implementation:  
```csharp
public class InMemoryCustodian
{
    public Dictionary<string, InMemoryRecorder> Data = [];
    public List<string> StartingEvidence = [];
    public Custodian Build() =>
        new()
        {
            Recorder = fn =>
            {
                if (!Data.ContainsKey(fn))
                    Data[fn] = new InMemoryRecorder();
                return Data[fn];
            },
            Retriever = fn => StartingEvidence
        };
}

public class InMemoryRecorder : IEvidenceRecorder
{
    public bool Cleared;
    public List<string> Values = [];
    public void Append(string evidence) => Values.Add(evidence);
    public void Clear() { Cleared = true; Values.Clear(); }
}
```
This one stores evidence and reports in memory, and is used in the tests to avoid touching the filesystem  
## ShrinkMode
Controls which shrinking strategies are active.

The following Checkr will be used to generate the reports throughout this chapter:  
```csharp
    from counter in Checkr.Input("counter", Fuzzr.Counter("counter"))
    from irrelevant in Checkr.Input("Irrelevant", Fuzzr.Int(1, 25))
    from expect in Checkr.Expect("Counter does not equal 3", () => counter != 3)
    select Case.Closed;
```
```csharp
private static CheckrConfig ConfigureNone(CheckrConfig config)
    => config with { ShrinkMode = ShrinkMode.None };
```
#### ShrinkMode.None
QuickCheckr reports the original failing run.  
```text
------------------------------------------------------------
 Test:                    RunTestNone
 Location:                Docs\B_PuttingACaseOnTheBoard\C_Configuration\D_ShrinkMode\ShrinkModeTests.cs:44:1
 Original failing run:    3 executions
 Seed:                    1044984793
 ------------------------------------------------------------
  Executed:
   - Input: counter    = 1
   - Input: Irrelevant = 13
 ------------------------------------------------------------
  Executed:
   - Input: counter    = 2
   - Input: Irrelevant = 5
 ------------------------------------------------------------
  Executed:
   - Input: counter    = 3
   - Input: Irrelevant = 8
 ===================================================
  !! Expectation Failed: Counter does not equal 3
 ===================================================
 Passed Expectations
 - Counter does not equal 3: 2x
 ------------------------------------------------------------
```
#### ShrinkMode.Executions
Trims away irrelevant executions.  
```csharp
private static CheckrConfig ConfigureExecutions(CheckrConfig config)
    => config with { ShrinkMode = ShrinkMode.Executions };
```
```text
------------------------------------------------------------
 Test:                    RunTestExecutions
 Location:                Docs\B_PuttingACaseOnTheBoard\C_Configuration\D_ShrinkMode\ShrinkModeTests.cs:89:1
 Original failing run:    3 executions
 Minimal failing case:    1 execution (after 2 shrinks)
 Seed:                    1044984793
 ------------------------------------------------------------
  Executed:
   - Input: counter    = 3
   - Input: Irrelevant = 8
 ===================================================
  !! Expectation Failed: Counter does not equal 3
 ===================================================
 Passed Expectations
 - Counter does not equal 3: 2x
 ------------------------------------------------------------
```
#### ShrinkMode.Inputs
Trims inputs to find a minimal failing case.  
```csharp
private static CheckrConfig ConfigureInputs(CheckrConfig config)
    => config with { ShrinkMode = ShrinkMode.Inputs };
```
```text
------------------------------------------------------------
 Test:                    RunTestInputs
 Location:                Docs\B_PuttingACaseOnTheBoard\C_Configuration\D_ShrinkMode\ShrinkModeTests.cs:124:1
 Original failing run:    3 executions
 Minimal failing case:    3 executions (after 5 shrinks)
 Seed:                    1044984793
 ------------------------------------------------------------
  Executed (2 Times)
 ------------------------------------------------------------
  Executed:
   - Input: counter = 3
 ===================================================
  !! Expectation Failed: Counter does not equal 3
 ===================================================
 Passed Expectations
 - Counter does not equal 3: 2x
 ------------------------------------------------------------
```
#### ShrinkMode.Reduction
Applies configured reducers for fine-grained shrinking.  
```csharp
private static CheckrConfig ConfigureReduction(CheckrConfig config)
    => config with { ShrinkMode = ShrinkMode.Reduction };
```

!! WARNING: **Experimental Feature** !!

> This feature is **experimental**.  
> Its API, behaviour, or semantics may change substantially in future versions.  
>  
> Use with caution in production code.  
> It is included for users who want to experiment and help shape the future of QuickCheckr.

  
*Note:* As of now reporting with only this mode active is not fully supported.  
#### ShrinkMode.Default
Includes all possible ShrinkModes.  
```text
------------------------------------------------------------
 Test:                    DefaultSetting
 Location:                Docs\B_PuttingACaseOnTheBoard\C_Configuration\D_ShrinkMode\ShrinkModeTests.cs:182:1
 Original failing run:    3 executions
 Minimal failing case:    1 execution (after 3 shrinks)
 Seed:                    1044984793
 ------------------------------------------------------------
  Executed:
   - Input: counter = 3
 ===================================================
  !! Expectation Failed: Counter does not equal 3
 ===================================================
 Passed Expectations
 - Counter does not equal 3: 2x
 ------------------------------------------------------------
```
## ReportMode
Controls whether or not *Traces*, *Warnings*  and/or *Exception StackTraces* show up in the report.

The following Checkr will be used to generate the reports throughout this chapter:  
```csharp
    from counter in Checkr.Input("Counter", Fuzzr.Counter("counter"))
    from irrelevant in Checkr.Input("Irrelevant", Fuzzr.Int())
    from nullThing in Checkr.Input("Null Thing", Fuzzr.Constant((Thing)null!))
    from trace in Checkr.Trace("Irrelevant", () => irrelevant)
    from boom in Checkr.Act("Blows up on 2", () => counter != 2 || ComputerSays.No<bool>("Only one is available"))
    select Case.Closed;
```
### ReportMode.Default
```csharp
protected override CheckrConfig Configure(CheckrConfig config)
    => config with { ReportMode = ReportMode.Default };
```
Includes all possible ReportModes.  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                ReportModeDefault.cs:43:1
 Original failing run:    2 executions
 Minimal failing case:    1 execution (after 2 shrinks)
 Seed:                    1964562926
 ------------------------------------------------------------
  Executed: Blows up on 2
   - Input: Counter    = 2
   - Input: Null Thing = null
   - Trace: Irrelevant = 73
   - WARNING: Null value for type 'Thing' at input 'Null Thing'.
 ===========================================================================
  !! QuickPulse.Instruments.ComputerSaysNo: Only one is available
   at QuickPulse.Instruments.ComputerSays.No[T](String msg)
   at QuickCheckr.Tests.Docs.B_PuttingACaseOnTheBoard.C_Configuration.E_ReportMode.ReportModeTests.<>c__DisplayClass1_1.<GetCheckr>b__9() in C:\Code\QuickCheckr\QuickCheckr.Tests\Docs\B_PuttingACaseOnTheBoard\C_Configuration\E_ReportMode\ReportModeTests.cs:line 38
   at QuickCheckr.Checkr.<>c__DisplayClass10_1`1.<TryCatch>b__1() in C:\Code\QuickCheckr\QuickCheckr\CheckrAct_Helper.cs:line 12
   at QuickCheckr.UnderTheHood.TheCatcher.InTheCheckr[T](CheckrState state, String key, Func`1 func, Action`1 onException) in C:\Code\QuickCheckr\QuickCheckr\UnderTheHood\TheCatcher.cs:line 15
 ===========================================================================
```
### ReportMode.None
```csharp
protected override CheckrConfig Configure(CheckrConfig config)
    => config with { ReportMode = ReportMode.None };
```
No traces, no warnings, no labels, no stacktrace.  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                ReportModeNone.cs:43:1
 Original failing run:    2 executions
 Minimal failing case:    1 execution (after 2 shrinks)
 Seed:                    1964562926
 ------------------------------------------------------------
  Executed: Blows up on 2
   - Counter    = 2
   - Null Thing = null
 ===========================================================================
  !! ComputerSaysNo: Only one is available
 ===========================================================================
```
### ReportMode.Trace
```csharp
protected override CheckrConfig Configure(CheckrConfig config)
    => config with { ReportMode = ReportMode.Trace };
```
Only traces.  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                ReportModeTrace.cs:43:1
 Original failing run:    2 executions
 Minimal failing case:    1 execution (after 2 shrinks)
 Seed:                    1964562926
 ------------------------------------------------------------
  Executed: Blows up on 2
   - Counter    = 2
   - Null Thing = null
   - Irrelevant = 73
 ===========================================================================
  !! ComputerSaysNo: Only one is available
 ===========================================================================
```
### ReportMode.Warning
```csharp
protected override CheckrConfig Configure(CheckrConfig config)
    => config with { ReportMode = ReportMode.Warning };
```
Only warnings.  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                ReportModeWarning.cs:43:1
 Original failing run:    2 executions
 Minimal failing case:    1 execution (after 2 shrinks)
 Seed:                    1964562926
 ------------------------------------------------------------
  Executed: Blows up on 2
   - Counter    = 2
   - Null Thing = null
   - WARNING: Null value for type 'Thing' at input 'Null Thing'.
 ===========================================================================
  !! ComputerSaysNo: Only one is available
 ===========================================================================
```
### ReportMode.Labels
```csharp
protected override CheckrConfig Configure(CheckrConfig config)
    => config with { ReportMode = ReportMode.Labels };
```
Only labels.  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                ReportModeLabels.cs:43:1
 Original failing run:    2 executions
 Minimal failing case:    1 execution (after 2 shrinks)
 Seed:                    1964562926
 ------------------------------------------------------------
  Executed: Blows up on 2
   - Input: Counter    = 2
   - Input: Null Thing = null
 ===========================================================================
  !! ComputerSaysNo: Only one is available
 ===========================================================================
```
**Hint:** You can use bit-wise operations to manipulate `ReportMode`:  
```csharp
config with { ReportMode = config.ReportMode & ~ReportMode.StackTrace };
```
## Deliberation Policy
Sometimes the shrinker comes up with a *local* minimal fail-case, not *the* minimal fail-case.  
Configuring a *DeliberationPolicy* allows us to instruct the Shrinker to explore the input further in a directed fasion.  

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
 Location:                Docs\B_PuttingACaseOnTheBoard\C_Configuration\F_DeliberationPolicy\DeliberationPolicy.cs:123:1
 Original failing run:    5 executions
 Minimal failing case:    2 executions (after 3 shrinks)
 Seed:                    146231062
 ------------------------------------------------------------
  Executed: Deposit
   - Input: Amount = 6
 ------------------------------------------------------------
  Executed: Withdraw
   - Input: Amount = 5
 =======================================
  !! Expectation Failed: No Overdraft
 =======================================
 Passed Expectations
 - No Overdraft: 4x
 ------------------------------------------------------------
```
The following predefined policies are available  
```csharp
- DeliberationPolicy.MinimumExecutions
- DeliberationPolicy.MinimumShrinkCount
- DeliberationPolicy.MaximumShrinkCount
- DeliberationPolicy.MinimumWarnings
- DeliberationPolicy.MaximumExecutionsShrunk
- DeliberationPolicy.MaximumDistinctPassedExpectations
```

See the documentation for [*Deliberation Shrinking*][DeliberationShrinking] for more examples and usage of these policies.

It is also possible to define a custom policy, but this is an ...  

!! WARNING: **Experimental Feature** !!

> This feature is **experimental**.  
> Its API, behaviour, or semantics may change substantially in future versions.  
>  
> Use with caution in production code.  
> It is included for users who want to experiment and help shape the future of QuickCheckr.

  

[DeliberationShrinking]: ../../../H_OnShrinking/F_DeliberationShrinking/DeliberationShrinking.md
## Internal Affairs
The *clue* is in the name. This is used to debug QuickCheckr itself when things go awry, and as such it is an ...  

!! WARNING: **Experimental Feature** !!

> This feature is **experimental**.  
> Its API, behaviour, or semantics may change substantially in future versions.  
>  
> Use with caution in production code.  
> It is included for users who want to experiment and help shape the future of QuickCheckr.

  
It might evolve into something that is *open to the public*, or it might get removed entirely, ... not sure.  
## Hint
In order to avoid having to deal with the configuration all the time, in case you do not like the defaults, for instance,
it is advised to simply define your own `CheckrOf` extension method(s):  
  
**Fast Deliberation:**  
```csharp
public static CaseFile Deliberate(
    this CheckrOf<Case> checkr,
    Func<CaseFile, CaseFile, CaseFile>? policy = null)
    => checkr.Run(50.Runs(), 20.ExecutionsPerRun(),
        cfg => cfg with { DeliberationPolicy = policy ?? DeliberationPolicy.MinimumExecutions });
```
**Auto File:**  
```csharp
public static CaseFile RunAndFile(
    this CheckrOf<Case> checkr,
    [CallerFilePath] string filePath = "",
    [CallerMemberName] string testName = "")
    => checkr.Run(50.Runs(), 20.ExecutionsPerRun(),
         cfg => cfg with { FileAs = $"{Path.GetFileName(filePath)}-{testName}" });
```
**Knock on some doors:**  
```csharp
public static void Investigate(
    this CheckrOf<Case> checkr,
    [CallerFilePath] string filePath = "",
    [CallerMemberName] string testName = "")
    => checkr.GatherEvidence(100.Investigations(), 10.Runs(), 20.ExecutionsPerRun(),
        cfg => cfg with { FileAs = $"{Path.GetFileName(filePath)}-{testName}" });
```
You get the idea.  

[Up][PuttingACaseOnTheBoard] | [Previous][RunningACheckr] | [Next][TheEvidenceVault]  

[PuttingACaseOnTheBoard]: ../PuttingACaseOnTheBoard.md

[RunningACheckr]: ../B_RunningACheckr/RunningACheckr.md

[TheEvidenceVault]: ../D_TheEvidenceVault/TheEvidenceVault.md
