## Putting a Case on the Board
This chapter takes you through the different options on how to run, configure and rerun Checkrs.
It also explains the *Evidence Vault* system and how to gather evidence.
  
### Test Framework Integration
QuickCheckr is fully **framework agnostic**: it doesn't care whether you run your tests with xUnit, NUnit, MSTest, or TUnit. There are no base classes to inherit from, no runner integrations to install, and no magic attributes controlling behaviour.  

A Checkr is just a LINQ expression that you evaluate by calling `Run()`.  

If a minimal failing case is found, QuickCheckr reports it by **throwing a `FalsifiableException`**, which contains the complete shrunk report, including the seed, execution steps, inputs, actions, traces, and the reason for failure.  

Because the failure is surfaced as a standard exception, any test framework can treat it as a normal assertion failure and you get a rich, human-readable report for free.  
### Contents

- [Running a Checkr][RunningACheckr]
- [Configuration][Configuration]
- [The Evidence Vault][TheEvidenceVault]
- [Gathering Evidence][GatheringEvidence]
  

[RunningACheckr]: #running-a-checkr

[Configuration]: #configuration

[TheEvidenceVault]: #the-evidence-vault

[GatheringEvidence]: #gathering-evidence
### Running a Checkr
[The Board][PuttingACaseOnTheBoard] | [Next][Configuration]  
  
This is done by calling `Run` on the Checkr instance.  
Several overloads are available:  
- Run the Checkr once with a single execution.  
```csharp
checkr.Run();
```
- Run the Checkr multiple times, each consisting of a single execution.  
```csharp
checkr.Run(5.Runs());
```
- Run the Checkr once but performs multiple executions within that run.  
```csharp
checkr.Run(5.ExecutionsPerRun());
```
- Run the Checkr with the specified number of runs and executions per run.  
```csharp
checkr.Run(3.Runs(), 5.ExecutionsPerRun());
```
- Run the Checkr with the specified seed and one execution.  
```csharp
checkr.Run(424242);
```
- Run the Checkr with the specified seed and the specified number of executions.  
```csharp
checkr.Run(424242, 5.ExecutionsPerRun());
```
All of the methods mentioned above can take an optional parameter `Func<CheckrConfig, CheckrConfig> configure`
which allows for controlling specific aspects of the Checkr run.  

[The Board][PuttingACaseOnTheBoard] | [Previous][TestFrameworkIntegration] | [Next][Configuration]  

[Configuration]: #configuration

[PuttingACaseOnTheBoard]: #putting-a-case-on-the-board
### Configuration
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
    MaxStepsPerReducer = 1024
}
```
#### Filing Evidence
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
#### The Custodian
`TheCustodian` is in charge of storing and retrieving QuickCheckr artifacts.  
As we saw in 'Filing Evidence', QuickCheckr stores reports and failing seeds on disk.  
This is the default behaviour, but by configuring the `Custodian` we can change this behaviour.

Currently only one extra `Custodian` is provided: `Custodian.Local()`.  
This one stores everything in a '.quickcheckr' directory in the same directory as the test file that created it.
  
##### Custom Custodians
You can also supply your own implementation:  
```csharp
namespace QuickCheckr.Tests.Drafts.B_PuttingACaseOnTheBoard.C_Configuration.B_TheCustodian;

public class InMemoryCustodian(HashSet<VaultEntry> StartingEvidence) : ICustodian
{
    public Dictionary<string, CaseFile> CaseFiles { get; private set; } = [];

    public void StoreCaseFile(string path, CaseFile caseFile, Func<CaseFile, Flow<Flow>> styleGuide)
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
#### Deliberation Policy
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
#### ReportMode
You can use this to customize report formatting somewhat.  
I personally almost always disable the *Labels* ('Input:', 'Pool:', etc...), but your milage may vary.  

It is also useful to toggle traces on or off, for instance.  
```csharp
private static CheckrConfig DisableTraces(CheckrConfig config) =>
    config with { ReportMode = config.ReportMode & ~ReportMode.Trace };
```
A full reference to the available options can be found [here][A_ReportModeReference].  
#### ShrinkMode
Controls which shrinking strategies are active.  

This is an option you will likely never need.  
It's main use is to investigate what happened during shrinking.  
As such, disabling shrinking entirely is probably the most useful step.  
```csharp
private static CheckrConfig DisableShrinking(CheckrConfig config) =>
    config with { ShrinkMode = ShrinkMode.None };
```
A full reference to the available options can be found [here][A_ShrinkModeReference].  
#### Hint
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
    => checkr.Conduct(100.Investigations(), 10.Runs(), 20.ExecutionsPerRun(),
        cfg => cfg with { FileAs = $"{Path.GetFileName(filePath)}-{testName}" });
```
You get the idea.  

[PuttingACaseOnTheBoard]: #putting-a-case-on-the-board

[RunningACheckr]: #running-a-checkr

[TheEvidenceVault]: #the-evidence-vault

[A_ReportModeReference]: #reportmode-reference

[A_ShrinkModeReference]: #shrinkmode-reference
##### ReportMode Reference
The following Checkr will be used to generate the reports throughout this chapter:  
```csharp
from counter in Checkr.Input("Counter", Fuzzr.Counter("counter"))
from irrelevant in Checkr.Input("Irrelevant", Fuzzr.Int())
from nullThing in Checkr.Input("Null Thing", Fuzzr.Constant((Thing)null!))
from trace in Checkr.Trace("Irrelevant", () => irrelevant)
from boom in Checkr.Act("Blows up on 2", () => counter != 2 || ComputerSays.No<bool>("Only one is available"))
select Case.Closed;
```
###### ReportMode.Default
```csharp
protected override CheckrConfig Configure(CheckrConfig config)
    => config with { ReportMode = ReportMode.Default };
```
Includes all possible ReportModes, except `ReportMode.FinalTrace`.  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                ReportModeDefault.cs:68:1
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
   at QuickCheckr.Tests.Drafts.B_PuttingACaseOnTheBoard.C_Configuration.J_ReportMode.Reference.A_ReportModeReference.<>c__DisplayClass1_1.<GetCheckr>b__9() in C:\Code\QuickCheckr\QuickCheckr.Tests\Docs\B_PuttingACaseOnTheBoard\C_Configuration\J_ReportMode\Reference\A_ReportModeReference.cs:line 33
   at QuickCheckr.Checkr.<>c__DisplayClass10_1`1.<TryCatch>b__1() in C:\Code\QuickCheckr\QuickCheckr\CheckrAct_Helper.cs:line 12
   at QuickCheckr.UnderTheHood.TheCatcher.InTheCheckr[T](CheckrState state, String key, Func`1 func, Action`1 onException) in C:\Code\QuickCheckr\QuickCheckr\UnderTheHood\TheCatcher.cs:line 15
 ===========================================================================
```
###### ReportMode.None
```csharp
protected override CheckrConfig Configure(CheckrConfig config)
    => config with { ReportMode = ReportMode.None };
```
No traces, no warnings, no labels, no stacktrace.  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                ReportModeNone.cs:68:1
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
###### ReportMode.Trace
```csharp
protected override CheckrConfig Configure(CheckrConfig config)
    => config with { ReportMode = ReportMode.Trace };
```
Only traces.  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                ReportModeTrace.cs:68:1
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
###### ReportMode.Warning
```csharp
protected override CheckrConfig Configure(CheckrConfig config)
    => config with { ReportMode = ReportMode.Warning };
```
Only warnings.  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                ReportModeWarning.cs:68:1
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
###### ReportMode.Labels
```csharp
protected override CheckrConfig Configure(CheckrConfig config)
    => config with { ReportMode = ReportMode.Labels };
```
Only labels.  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                ReportModeLabels.cs:68:1
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
###### ReportMode.FinalTrace
Using a different checkr in order to demonstrate the difference between `ReportMode.Trace` and `ReportMode.FinalTrace`.
- `ReportMode.Trace` reports the values of the initial run.
- `ReportMode.FinalTrace` reports the values of the final run, after shrinking.  
```csharp
protected override CheckrConfig Configure(CheckrConfig config)
    => config with { ReportMode = ReportMode.FinalTrace };
```
```text
------------------------------------------------------------
 Test:                    Example
 Location:                ReportModeFinalTrace.cs:51:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    1964562926
 ------------------------------------------------------------
  Executed:
   - Input = 59
     Redux   10
   - Input / 2 = 5
 =====================================
  !! Expectation Failed: Input > 10
 =====================================
```
**Hint:** You can use bit-wise operations to manipulate `ReportMode`:  
```csharp
config with { ReportMode = config.ReportMode & ~ReportMode.StackTrace };
```
##### ShrinkMode Reference
The following Checkr will be used to generate the reports throughout this chapter:  
```csharp
from counter in Checkr.Input("counter", Fuzzr.Counter("counter"))
from relevant in Checkr.Input("Relevant", Fuzzr.Int(), Shrink.OnType<int>(s => s.Simplify(Reduce.TowardsZero)))
from irrelevant in Checkr.Input("Irrelevant", Fuzzr.Int(1, 25))
from expect in Checkr.Expect("Third Time Answer", () => !(counter == 3 && relevant >= 42))
select Case.Closed;
```
###### ShrinkMode.None
```csharp
protected override CheckrConfig Configure(CheckrConfig config)
    => config with { ShrinkMode = ShrinkMode.None };
```
QuickCheckr reports the original failing run.  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                ShrinkModeNone.cs:68:1
 Original failing run:    3 executions
 Seed:                    2145473476
 ------------------------------------------------------------
  Executed:
   - Input: counter    = 1
   - Input: Relevant   = 51
   - Input: Irrelevant = 6
 ------------------------------------------------------------
  Executed:
   - Input: counter    = 2
   - Input: Relevant   = 38
   - Input: Irrelevant = 19
 ------------------------------------------------------------
  Executed:
   - Input: counter    = 3
   - Input: Relevant   = 66
   - Input: Irrelevant = 13
 ============================================
  !! Expectation Failed: Third Time Answer
 ============================================
 Passed Expectations
 - Third Time Answer: 2x
 ------------------------------------------------------------
```
###### ShrinkMode.Executions
```csharp
protected override CheckrConfig Configure(CheckrConfig config)
    => config with { ShrinkMode = ShrinkMode.Executions };
```
Trims away irrelevant executions.  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                ShrinkModeExecutions.cs:68:1
 Original failing run:    3 executions
 Minimal failing case:    1 execution (after 2 shrinks)
 Seed:                    2145473476
 ------------------------------------------------------------
  Executed:
   - Input: counter    = 3
   - Input: Relevant   = 66
   - Input: Irrelevant = 13
 ============================================
  !! Expectation Failed: Third Time Answer
 ============================================
 Passed Expectations
 - Third Time Answer: 2x
 ------------------------------------------------------------
```
###### ShrinkMode.Inputs
```csharp
protected override CheckrConfig Configure(CheckrConfig config)
    => config with { ShrinkMode = ShrinkMode.Inputs };
```
Trims inputs to find a minimal failing case.  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                ShrinkModeInputs.cs:68:1
 Original failing run:    3 executions
 Minimal failing case:    3 executions (after 7 shrinks)
 Seed:                    2145473476
 ------------------------------------------------------------
  Executed: (2 Times)
   - WARNING: All inputs were considered irrelevant.
 ------------------------------------------------------------
  Executed:
   - Input: counter  = 3
   - Input: Relevant = 66
 ============================================
  !! Expectation Failed: Third Time Answer
 ============================================
 Passed Expectations
 - Third Time Answer: 2x
 ------------------------------------------------------------
```
###### ShrinkMode.Default
```csharp
protected override CheckrConfig Configure(CheckrConfig config)
    => config with { ShrinkMode = ShrinkMode.Default };
```
Includes `ShrinkMode.Executions` and `ShrinkMode.Inputs`.  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                ShrinkModeDefault.cs:68:1
 Original failing run:    3 executions
 Minimal failing case:    1 execution (after 4 shrinks)
 Seed:                    2145473476
 ------------------------------------------------------------
  Executed:
   - Input: counter  = 3
   - Input: Relevant = 66
            Redux      42
 ============================================
  !! Expectation Failed: Third Time Answer
 ============================================
 Passed Expectations
 - Third Time Answer: 2x
 ------------------------------------------------------------
```
**Hint:** You can use bit-wise operations to manipulate `ShrinkMode`:  
```csharp
config with { ShrinkMode = config.ShrinkMode & ~ShrinkMode.Inputs };
```
### The Evidence Vault
[The Board][PuttingACaseOnTheBoard] | [Previous][Configuration] | [Next][GatheringEvidence]  
  
#### Reviewing Cold Cases
Once an evidence vault exists the seeds therein contained can be reviewed.  
This is done by calling `ReviewColdCases` on the Checkr instance, passing in the same configuration used to create the evidence vault.  
```csharp
checkr.ReviewColdCases(Configure);
```
This will create a report per seed in the evidence vault and a log of all evidence vault actions.  
- For every failing seed the report will look like the by now familiar failure report.  
- For every passing seed the report will only contain a list of passed expectations.  
#### Closing Cold Cases
The method mentioned above is usefull for regression testing, but sometimes,
there is no need to keep the evidence around after the culprit is found.  
In this case we use:  
```csharp
checkr.CloseResolvedColdCases(Configure);
```
If the seeds stored in the vault still all result in failure, the behaviour is exactly the same as `ReviewColdCases`.  
If however a run for a given seed no longer fails, the seed is removed from the evidence vault,
and no report is generated for the passing run.  
Any existing previous reports for runs that now pass are deleted, and if the evidence vault is empty, it is also deleted.  

[PuttingACaseOnTheBoard]: #putting-a-case-on-the-board

[Configuration]: #configuration

[GatheringEvidence]: #gathering-evidence
### Gathering Evidence
[The Board][PuttingACaseOnTheBoard] | [Previous][TheEvidenceVault] 
  
This is another way to run a Checkr, and an alternate method to populate the *Evidence Vault* with seeds for failing runs.  
#### Running Investigations
```csharp
checkr.Conduct(50.Investigations(), 3.Runs(), 10.ExecutionsPerRun(),
    cfg => cfg with { FileAs = "GatheringEvidence" });
```
When you use the above method the following steps are taken:
- The existing evidence from the vault is loaded (if available), the seeds are run, and the failing casefiles are kept in memory.
- Next we run the specified number of Investigations, and any new failing case file is added to the in-memory list.
- A DeskOfficer compares the casefiles for similarities, only keeping the different ones.
- And finally, all evidence and reports are saved to the vault.

**Note:** An *Investigation* is a new kind of scope.  
Running a Checkr in the *normal* way will always produce one report, even if used with a DeliberationPolicy.  
When you're gathering evidence, a report is generated per Investigation. This allows for finding different bugs in the same system,
or for documenting the different ways a single bug can manifest.

As stated above, similar CaseFiles are not reported in order to increase the signal to noise ratio.  
  
**Casefiles are considered *not similar* when:**  
- *Failure Description* is not equal.  
- *Execution Count* is not equal.  
- Any **Execution** differs, like so:  
  - Number of *Actions Performed* is not equal.  
  - Any *Action label* is not equal.  
  - Number of *Inputs* is not equal.  
  - Any *Input label* is not equal.  

Per default, the order of Executions does not matter for similarity.  
An optional boolean parameter to the `GatherEvidence` method allows one to override this default behaviour, so that Execution order **does** matter.  
#### Show, Don't Tell
As stated in [the guide][Guide] we're going to use the [Bolting Horses][BoltingHorses] example,
to demonstrate how to find all the different ways the invariant can be violated.

So we're using the same model, and *almost* exactly the same Checkr.  
Changes:
- Added Shrink.None to the Name inputs. 
- Updated `UpdateCoachSkills` to avoid the issue documented [here][J_TheCaseOfTheInvisibleCollection]:  
```csharp
public class GatheringEvidenceCheckr
{
    public static CheckrOf<Case> TheCheckr() =>
        from operations in Checkr.OneOfWhen(
            Trackr.PoolWhen<Coach>(a => a.Count < 10, CreateCoach),
            Trackr.PoolWhen("Coach to Update Skills", a => a.Count != 0, UpdateCoachSkills),
            Trackr.PoolWhen<Course>(a => a.Count < 5, CreateCourse),
            Trackr.PoolWhen("Course to Update Skills", a => a.Count != 0, UpdateCourseSkills),
            Trackr.PoolWhen("(Coach, Course) to Assign",
                (coach, course) => coach.IsSuitableFor(course), AssignCoachToCourse))
        from invariant in AssignedCoachesMustBeSuitable
        select Case.Closed;

    private static readonly CheckrOf<Case> CreateCoach =
        from name in Checkr.Input("Coach Name", NameFuzzr.For("Coach"),
            Shrink.None<string>())
        from coach in Checkr.Act("Create Coach", () => new Coach(name))
        from checkName in Checkr.Expect("Coach Name is set", () => coach.Name == name)
        from toPool in Trackr.ToPool("Store Coach", coach)
        select Case.Closed;

    private static readonly Func<PoolElement<Coach>, CheckrOf<Case>> UpdateCoachSkills =
        coach =>
            from skills in Checkr.Input("Coach Skills"
                , Fuzzr.OneOf(Skill.Pool).Unique(Guid.NewGuid()).Many(1, 3)
                , Shrink.CollectionFromState(coach.Value.Skills, []))
            from updateSkills in Checkr.Act("Update Coach Skills",
                () => coach.Value.UpdateSkills(skills))
            from checkSkills in Checkr.Expect("Coach Skills Set",
                () => coach.Value.Skills.Count
                   == skills.Count() && coach.Value.Skills.All(skills.Contains))
            select Case.Closed;

    private static readonly CheckrOf<Case> CreateCourse =
        from name in Checkr.Input("Course Name", NameFuzzr.For("Course"),
            Shrink.None<string>())
        from course in Checkr.Act("Create Course", () => new Course(name))
        from checkName in Checkr.Expect("Course Name Set", () => course.Name == name)
        from toPool in Trackr.ToPool("Store Course", course)
        select Case.Closed;

    private static readonly Func<PoolElement<Course>, CheckrOf<Case>> UpdateCourseSkills =
        course =>
            from skills in Checkr.Input("Course Skills",
                Fuzzr.OneOf(Skill.Pool).Unique(Guid.NewGuid()).Many(1, 3))
            from updateSkills in Checkr.Act("Update Course Skills",
                () => course.Value.UpdateRequiredSkills(skills))
            from checkSkills in Checkr.Expect("Course Required Skills Set",
                () => course.Value.RequiredSkills.Count
                   == skills.Count() && course.Value.RequiredSkills.All(skills.Contains))
            select Case.Closed;

    private static readonly Func<PoolElement<Coach>, PoolElement<Course>, CheckrOf<Case>> AssignCoachToCourse =
        (coach, course) =>
            from assign in Checkr.Act("Assign Coach to Course",
                () => course.Value.AssignCoach(coach.Value))
            from isAssigned in Checkr.Expect("Coach is assigned",
                () => course.Value.AssignedCoach == coach.Value)
            from hasCourse in Checkr.Expect("Coach has Course",
                () => coach.Value.AssignedCourses.Contains(course.Value))
            select Case.Closed;

    private static readonly CheckrOf<Case> AssignedCoachesMustBeSuitable =
        from assignedCheck in Trackr.PoolExpectEachWhen<Coach>("Assigned Coach Must Be Suitable",
            coach => coach.AssignedCourses.Any(),
            coach => coach.AssignedCourses.All(course => coach.IsSuitableFor(course)))
        select Case.Closed;
}
```
For the code on display here, running this checkr using the code snippet at the top of this doc, 
consistently results in two different reports.  
```text
------------------------------------------------------------
 Original failing run:    10 executions
 Minimal failing case:    4 executions (after 11 shrinks)
 Seed:                    1652806413
 ------------------------------------------------------------
  Executed: Create Course
   - Store Course = Course-1
 ------------------------------------------------------------
  Executed: Create Coach
   - Store Coach = Coach-3
 ------------------------------------------------------------
  Executed: Assign Coach to Course
   - (Coach, Course) to Assign = ( Coach-3, Course-1 )
 ------------------------------------------------------------
  Executed: Update Course Skills
   - Course to Update Skills = Course-1
   - Course Skills           = [ _ ]
 ==========================================================
  !! Expectation Failed: Assigned Coach Must Be Suitable
 ==========================================================
 Passed Expectations
 - Coach Name is set: 3x
 - Coach Skills Set: 2x
 - Course Name Set: 1x
 - Course Required Skills Set: 3x
 - Coach is assigned: 1x
 - Coach has Course: 1x
 - Assigned Coach Must Be Suitable: 1x
 ------------------------------------------------------------
```
```text
------------------------------------------------------------
 Original failing run:    12 executions
 Minimal failing case:    6 executions (after 11 shrinks)
 Seed:                    414682113
 ------------------------------------------------------------
  Executed: Create Course
   - Store Course = Course-1
 ------------------------------------------------------------
  Executed: Update Course Skills
   - Course to Update Skills = Course-1
   - Course Skills           = [ "EF Core" ]
 ------------------------------------------------------------
  Executed: Create Coach
   - Store Coach = Coach-2
 ------------------------------------------------------------
  Executed: Update Coach Skills
   - Coach to Update Skills = Coach-2
   - Coach Skills           = [ "EF Core" ]
 ------------------------------------------------------------
  Executed: Assign Coach to Course
   - (Coach, Course) to Assign = ( Coach-2, Course-1 )
 ------------------------------------------------------------
  Executed: Update Coach Skills
   - Coach to Update Skills = Coach-2
   - Coach Skills           = [ "TDD" ]
     Redux                    [ ]
 ==========================================================
  !! Expectation Failed: Assigned Coach Must Be Suitable
 ==========================================================
 Passed Expectations
 - Course Name Set: 2x
 - Coach Name is set: 2x
 - Course Required Skills Set: 2x
 - Coach is assigned: 4x
 - Coach has Course: 4x
 - Assigned Coach Must Be Suitable: 6x
 - Coach Skills Set: 2x
 ------------------------------------------------------------
```
#### An Observation
When I first implemented this feature, I was quite puzzled.
Surely there are more ways this Checkr can fail, I thought.  

It took a bit of investigating as to why this is not the case. Let me give you an example of what is going on.

If you look at the second report, the one with 6 executions, surely there is a similar case,
but instead of updating the Coach skills, we update the Course skills and that would also fail.
Now that's a new way of failing the Checkr, right?

Wrong!  

Below is the report of the other failure (the one with 4 executions) with shrinking disabled,
but I'll save you the *bit of investigating* and give you a recap:
- Execution 03: Create Course-1
- Execution 06: Create Coach-3
- Execution 07: Update Coach Skills to [ "DomainDrivenDesign", "JavaScript" ]
- Execution 08: Update Course Skills to [ "DomainDrivenDesign" ]
- Execution 09: Assign Coach-3 to Course-1
- Execution 10: Update Course Skills to [ "JavaScript", "TDD", "CI/CD" ]

As you can see, this is exactly the case described above.  
The Shrinker however managed to collapse that one down to only four executions, so it is in fact covered by the first report.
  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Docs\B_PuttingACaseOnTheBoard\E_GatheringEvidence\GatheringEvidenceObservation.cs:37:1
 Original failing run:    10 executions
 Seed:                    1652806413
 ------------------------------------------------------------
  Executed: Create Coach
   -  Pool: Store Coach = Coach-1
   - Input: Coach Name  = "Coach-1"
 ------------------------------------------------------------
  Executed: Update Coach Skills
   -  Pool: Coach to Update Skills = Coach-1
   - Input: Coach Skills           = [ "ASP.NET" ]
 ------------------------------------------------------------
  Executed: Create Course
   -  Pool: Store Course = Course-1
   - Input: Course Name  = "Course-1"
 ------------------------------------------------------------
  Executed: Create Coach
   -  Pool: Store Coach = Coach-2
   - Input: Coach Name  = "Coach-2"
 ------------------------------------------------------------
  Executed: Update Course Skills
   -  Pool: Course to Update Skills = Course-1
   - Input: Course Skills           = [ "UnitTesting" ]
 ------------------------------------------------------------
  Executed: Create Coach
   -  Pool: Store Coach = Coach-3
   - Input: Coach Name  = "Coach-3"
 ------------------------------------------------------------
  Executed: Update Coach Skills
   -  Pool: Coach to Update Skills = Coach-3
   - Input: Coach Skills           = [ "DomainDrivenDesign", "JavaScript" ]
 ------------------------------------------------------------
  Executed: Update Course Skills
   -  Pool: Course to Update Skills = Course-1
   - Input: Course Skills           = [ "DomainDrivenDesign" ]
 ------------------------------------------------------------
  Executed: Assign Coach to Course
   -  Pool: (Coach, Course) to Assign = ( Coach-3, Course-1 )
 ------------------------------------------------------------
  Executed: Update Course Skills
   -  Pool: Course to Update Skills = Course-1
   - Input: Course Skills           = [ "JavaScript", "TDD", "CI/CD" ]
 ==========================================================
  !! Expectation Failed: Assigned Coach Must Be Suitable
 ==========================================================
 Passed Expectations
 - Coach Name is set: 3x
 - Coach Skills Set: 2x
 - Course Name Set: 1x
 - Course Required Skills Set: 3x
 - Coach is assigned: 1x
 - Coach has Course: 1x
 - Assigned Coach Must Be Suitable: 1x
 ------------------------------------------------------------
```

[PuttingACaseOnTheBoard]: #putting-a-case-on-the-board

[TheEvidenceVault]: #the-evidence-vault

[Guide]: #guide

[BoltingHorses]: #bolting-horses

[J_TheCaseOfTheInvisibleCollection]: #the-case-of-the-invisible-collection
## Building a Testing DSL with QuickCheckr
### Model Based Testing

**The Model:**  
```csharp
public class CalculatorModel
{
    public int Result { get; private set; } = 0;
    public void Add(int a) => Result += a;
    public void Subtract(int a) => Result -= a;
    public void Clear() => Result = 0;
}
```

**SUT:**  
```csharp
public class Calculator
{
    public int Result { get; private set; } = 0;
    private int counter = 0;
    public void Add(int a)
    {
        counter++;
        Result += a;
    }
    public void Subtract(int a) => Result -= a;
    public void Clear()
    {
        if (counter != 3)
            Result = 0;
    }
}
```
#### Using Linq

**The Checkr:**  
```csharp
from model in Trackr.Stashed(() => new CalculatorModel())
from sut in Trackr.Stashed(() => new Calculator())
from ops in Checkr.OneOf(
    from input in Checkr.Input("Input", Fuzzr.Int())
    from add in Checkr.Act("Add", () =>
    {
        model.Add(input);
        sut.Add(input);
    })
    select Case.Closed,
    from input in Checkr.Input("Input", Fuzzr.Int())
    from add in Checkr.Act("Subtract", () =>
    {
        model.Subtract(input);
        sut.Subtract(input);
    })
    select Case.Closed,
    Checkr.Act("Clear", () =>
    {
        model.Clear();
        sut.Clear();
    })
)
from observe in Checkr.Expect("Observe", () => model.Result == sut.Result)
from trace in Checkr.TraceWhen("Observation", () => model.Result != sut.Result, () => $"Model: {model.Result}, Sut: {sut.Result}.")
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                A_UsingLinq.cs:72:1
 Original failing run:    14 executions
 Minimal failing case:    4 executions (after 13 shrinks)
 Seed:                    1299945702
 ------------------------------------------------------------
  Executed: Add (3 Times)
   - WARNING: All inputs were considered irrelevant.
 ------------------------------------------------------------
  Executed: Clear
   - Trace: Observation = Model: 0, Sut: 196.
 ==================================
  !! Expectation Failed: Observe
 ==================================
 Passed Expectations
 - Observe: 13x
 ------------------------------------------------------------
```
#### Using Dsl

**The Checkr:**  
```csharp
Check
    .Model(() => new CalculatorModel())
    .Sut(() => new Calculator())
    .Operation("Add", Fuzzr.Int(),
        (model, a) => model.Add(a),
        (sut, a) => sut.Add(a))
    .Operation("Subtract", Fuzzr.Int(),
        (model, a) => model.Subtract(a),
        (sut, a) => sut.Subtract(a))
    .Operation("Clear",
        model => model.Clear(),
        sut => sut.Clear())
    .Report((model, sut) => $"Model: {model.Result}, Sut: {sut.Result}.")
    .Observe((model, sut) => model.Result == sut.Result);
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                B_UsingDsl.cs:50:1
 Original failing run:    7 executions
 Minimal failing case:    4 executions (after 6 shrinks)
 Seed:                    1879928073
 ------------------------------------------------------------
  Executed: Add (3 Times)
   - WARNING: All inputs were considered irrelevant.
 ------------------------------------------------------------
  Executed: Clear
   - Trace: Observation = Model: 0, Sut: 193.
 ==================================
  !! Expectation Failed: Observe
 ==================================
 Passed Expectations
 - Observe: 6x
 ------------------------------------------------------------
```
### Going Functional
Here are some examples from **QuickTestr**, which is a dsl built on top of **QuickCheckr**
facilitating easier creation of more traditional PBT tests.  
### Testr Parsing Numbers

**The Testr:**  
```csharp
Testr
    .Named("Parser matches golden model.")
    .For(ParseFuzzr.Expression(),
        Shrink.OnType<string>(
            s => s.Simplify(Reduce.Function<string>(ParseReducer.Reducer, true))))
    .DisableValueReduction()
    .Deliberate(a => a.Length, 4)
    .Assert(a => LostIn.Translation(a).Eval() == LostIn.FaultyTranslation(a).Eval());
```
`.Run()` is called without arguments, so it performs the default 100.Runs().  

**The Report:**  
```text
------------------------------------------------------------
  Parser matches golden model.
  Seed: 43650243
 ------------------------------------------------------------
  Falsified:
    Input = "- (2 - 1 / 3 + 2) ^ (1 / 1 * 3 - 1 / 3 / 1 + 1 * 3 / 2 / 1) / (2 - 1 / 1) / (1 + 1 / 2 * 3 - 2 / 1 * 1 + 1 * 3)"
    Redux = "-2^2"

  Original:
    "- (2 - 1 / 3 + 2) ^ (1 / 1 * 3 - 1 / 3 / 1 + 1 * 3 / 2 / 1) / (2 - 1 / 1) / (1 + 1 / 2 * 3 - 2 / 1 * 1 + 1 * 3)"
 ------------------------------------------------------------
```

**The Fuzzr:**  
```csharp
public class ParseFuzzr
{
    public static FuzzrOf<string> Expression(int maxDepth = 4) => Expr(maxDepth);
    public static readonly FuzzrOf<string> Number = from i in Fuzzr.Int(1, 4) select i.ToString();
    private static readonly FuzzrOf<string> AddOp = Fuzzr.OneOf("+", "-");
    private static readonly FuzzrOf<string> MulOp = Fuzzr.OneOf("*", "/");
    private static readonly FuzzrOf<string> PowOp = Fuzzr.Constant("^");
    private static readonly FuzzrOf<string> WS =
        from chs in Fuzzr.Constant(' ').Many(1)
        select new string([.. chs]);
    private static readonly FuzzrOf<string> UnaryOp = Fuzzr.OneOf("-");
    private static FuzzrOf<string> Primary(int maxDepth) =>
        maxDepth <= 0
        ? Number
        : Fuzzr.OneOf(
            Number,
            from l in Fuzzr.Constant("(")
            from e in Expr(maxDepth - 1)
            from r in Fuzzr.Constant(")")
            select l + e + r,
            from op in UnaryOp
            from ws in WS
            from inner in Primary(maxDepth - 1)
            select op + ws + inner
          );
    private static FuzzrOf<string> Factor(int maxDepth) =>
        from a in Primary(maxDepth)
        from tail in maxDepth <= 0
            ? Fuzzr.Constant("")
            : Fuzzr.OneOf(
                  Fuzzr.Constant(""),
                  from w1 in WS
                  from op in PowOp
                  from w2 in WS
                  from b in Factor(maxDepth - 1)
                  select w1 + op + w2 + b)
        select a + tail;
    private static FuzzrOf<string> Term(int maxDepth) =>
        from head in Factor(maxDepth)
        from pieces in (
            from w1 in WS
            from op in MulOp
            from w2 in WS
            from rhs in Factor(maxDepth - 1)
            select w1 + op + w2 + rhs
        ).Many(0, 3)
        select head + string.Concat(pieces);
    private static FuzzrOf<string> Expr(int maxDepth) =>
        from depth in Fuzzr.Int(0, maxDepth)
        from head in Term(depth)
        from pieces in (
            from w1 in WS
            from op in AddOp
            from w2 in WS
            from rhs in Term(depth - 1)
            select w1 + op + w2 + rhs
        ).Many(0, 3)
        select head + string.Concat(pieces);
}
```

**The Reducer:**  
```csharp
public static class ParseReducer
{
    public static IEnumerable<string> Reducer(string expr)
    {
        foreach (var item in RemoveOperations(expr))
            yield return item;
        string output = Regex.Replace(expr, @"\([^()]*\)", "2");
        yield return output;
        foreach (var item in RemoveOperations(output))
            yield return item;
    }
    private static IEnumerable<string> RemoveOperations(string expr)
    {
        var tokens = Lexer.Tokenize(expr);
        tokens = tokens.Where(a => a.Kind != TokenKind.LParen && a.Kind != TokenKind.RParen);
        yield return TokensToString(tokens);
        var list = tokens.ToList();
        for (int i = 0; i < list.Count; i++)
        {
            yield return TokensToString(tokens.Skip(i));
            if (list[i].IsOperator && !list[i + 1].IsOperator)
                yield return TokensToString(tokens.Skip(i + 1));
        }
        for (int i = list.Count - 1; i > 0; i--)
        {
            if (list[i].IsOperator && !list[i - 1].IsOperator)
                yield return TokensToString(tokens.Take(i));
        }
    }
    private static string TokensToString(IEnumerable<Token> tokens)
        => string.Join("", tokens.Select(a => a.Text));
}
```
## Testr Challenges

Currently these challenges use a seeded state, because they are part of a regression test suite.

At the time of writing however, this was not the case.
The seeded report *is* representing the actual behaviour of QuickTestr,
but for performance reasons we do not run deliberation policies everytime for instance.
  
### Bound5
Given a 5-tuple of lists of 16-bit integers, we want to test the property that if each list sums to less than 256,
then the sum of all the values in the lists is less than 5 * 256.
This is false because of overflow. e.g. ([-20000], [-20000], [], [], []) is a counter-example.

The interesting thing about this example is the interdependence between separate parts of the sample data.
A single list in the tuple will never break the invariant, but you need at least two lists together.
This prevents most of trivial shrinking algorithms from getting close to a minimum example,
which would look somethink like ([-32768], [-1], [], [], []).
  

**The Testr:**  
```csharp
Testr.Named("The sum of all values is less than 5 * 256.")
    .For(Fuzzr.Tuple(ShortList, ShortList, ShortList, ShortList, ShortList))
    .Deliberate(H.ElementCount, 2)
    .Assert(a => H.Sum(a) < (5 * 256));
```

**The Short List Fuzzr:**  
```csharp
Fuzzr.Short(short.MinValue, 256)
    .Many(0, 10)
    .Where(a => H.Sum(a) < 256)
    .ToList()
```

**The Helpers:**  
```csharp
public class H
{
    public static int ElementCount((List<short>, List<short>, List<short>, List<short>, List<short>) a)
        => a.Item1.Count + a.Item2.Count + a.Item3.Count + a.Item4.Count + a.Item5.Count;
    public static short Sum((List<short>, List<short>, List<short>, List<short>, List<short>) a)
        => (short)(Sum(a.Item1) + Sum(a.Item2) + Sum(a.Item3) + Sum(a.Item4) + Sum(a.Item5));
    public static short Sum(IEnumerable<short> list)
    {
        short sum = 0;
        foreach (var value in list)
            sum = (short)(sum + value);
        return sum;
    }
}
```

**The Report:**  
```text
------------------------------------------------------------
  The sum of all values is less than 5 * 256.
  Seed: 472166887
 ------------------------------------------------------------
  Falsified:
    Input = ( _, _, _, [ -23457 ], [ -25242 ] )
    Redux = ( _, _, _, [ -7527 ], [ -25242 ] )

  Original:
    ( [ ], [ ], [ ], [ -23457 ], [ -25242 ] )
 ------------------------------------------------------------
```
### Large Union List

Given a list of lists of arbitrary sized integers, 
we want to test the property that there are no more than four distinct integers across all the lists.
This is trivially false, and this example is an artificial one to stress test a shrinker's ability to normalise
(always produce the same output regardless of starting point).

In particular, a shrinker cannot hope to normalise this unless it is able to either split or join elements of the larger list.
For example, it would have to be able to transform one of [[0, 1, -1, 2, -2]] and [[0], [1], [-1], [2], [-2]] into the other.
  

**The Testr:**  
```csharp
Testr.Named("No more than four distinct integers.")
    .For(Fuzzr.Int().Many(0, 10).ToList().Many(5, 20).ToList())
    .Deliberate(a => 0 - a.Count, -5)
    .Assert(a => a.SelectMany(a => a).Distinct().Count() <= 4);
```

**The Report:**  
```text
------------------------------------------------------------
  No more than four distinct integers.
  Seed: 1575924946
 ------------------------------------------------------------
  Falsified:
    Input = [ [ _, 17 ], [ 51 ], [ 6 ], [ 74 ] ]
    Redux = [ [ _, 0 ], [ 1 ], [ 2 ], [ 3 ] ]

  Original:
    [ [ 91, 42, 27, 37, 85, 85, 40, 91, 75 ], [ 43, 58, 67 ], [ 78, 1, 82, 71, 59, 69, 81, 5 ], [ 56, 7, 12, 35 ], [ 74, 37, 40, 98, 14, 28 ], [ 26, 71, 93, 9, 98, 27 ], [ ], [ 78, 39, 94, 60 ], [ 52, 60 ], [ 30, 10, 9, 9, 87, 99 ], [ 30, 74, 50, 82, 18, 38, 93 ], [ 45, 40, 18, 52 ], [ 89, 62, 60 ], [ 79, 61, 63, 17 ], [ 51 ], [ 6 ], [ 74 ] ]
 ------------------------------------------------------------
```
### Reverse List

This tests the (wrong) property that reversing a list of integers results in the same list. 
It is a basic example to validate that a library can reliably normalize simple sample data.
  

**The Testr:**  
```csharp
Testr
    .Named("Reversing a list of integers results in the same list")
    .For(Fuzzr.Int().Many(0, 10).ToList())
    .Assert(a =>
    {
        var reversed = new List<int>(a);
        reversed.Reverse();
        return reversed.SequenceEqual(a);
    });
```

**The Report:**  
```text
------------------------------------------------------------
  Reversing a list of integers results in the same list
  Seed: 12901993
 ------------------------------------------------------------
  Falsified:
    Input = [ _, 94 ]
    Redux = [ _, 0 ]

  Original:
    [ 76, 92, 75, 6, 94 ]
 ------------------------------------------------------------
```
### Calculator

The challenge involves a simple calculator language representing expressions consisting of integers,
their additions and divisions only, like 1 + (2 / 3).

The property being tested is that

- if we have no subterms of the form x / 0,  
- then we can evaluate the expression without a zero division error.
This property is false, because we might have a term like 1 / (3 + -3), in which the divisor is not literally 0 but evaluates to 0.

One of the possible difficulties that might come up is the shrinking of recursive expressions.
  

**The Testr:**  
```csharp
Testr
    .Named("No division by zero")
    .For(TheFuzzr, TheShrinkr)
    .Format(TheFormatr)
    .Deliberate(a => DepthOf(a))
    .Assert(a => { a.Evaluate(); return true; });
```

**The Fuzzr:**  
```csharp
from i in Configr.Primitive(Fuzzr.Int(0, 10))
from noZeroDiv in Configr<Expr>.Apply(
    a =>
    {
        // rewrite division by zero terms
        if (a is Term t)
            if (t.Op == Operation.Div && t.R is Num { Value: 0 })
                t.R = new Num { Value = 1 };
    })
from depth in Configr<Expr>.Depth(0, 10)
from inheritance in Configr<Expr>.AsOneOf(typeof(Term), typeof(Num))
from terminator in Configr<Expr>.EndOn<Num>()
from expr in Fuzzr.One<Expr>()
select expr
```

**The Shrinkr:**  
```csharp
Shrink.When<int>(s => s
    .As<Num>(a => a.Value)
    .As<Term, Expr>(a => a.R)
    .Value(t => t.Op == Operation.Div),
s => s.Simplify(Reduce.Towards(1)))
```

**The Formatter:**  
```csharp
[ Showr.ForClassification(a => a.For<Term>()
    .Bounds("( ", " )")
    .Delimiter(" ")
    .HidePropertyNames()
    .Property(a => a.L)
    .Property(a => a.Op, a =>
        a switch
        {
            Operation.Add => "+",
            Operation.Div => "/",
            _ => throw new ArgumentOutOfRangeException()
        })
    .Property(a => a.R))
, Showr.ForClassification(a => a.For<Num>()
    .Bounds("", "")
    .Delimiter("")
    .HidePropertyNames())]
```

**The Helper:**  
```csharp
private static int DepthOf(Expr a, int depth = 0)
{
    if (a is Term term)
        return Math.Max(DepthOf(term.L, depth + 1), DepthOf(term.R, depth + 1));
    return depth;
}
```

**The Report:**  
```text
------------------------------------------------------------
  DivideByZeroException: Attempted to divide by zero.
  Seed: 729093046
 ------------------------------------------------------------
  Falsified:
    Input = ( _ / ( 2 / _ ) )
    Redux = ( _ / ( 0 / _ ) )

  Original:
    { Op: Div, L: { Value: 4 }, R: { Op: Div, L: { Value: 2 }, R: { Value: 3 } } }
 ------------------------------------------------------------
```
### Length List

A list should be generated first by picking a length between 1 and 100,
then by generating a list of precisely that length whose elements are integers between 0 and 1000.
The test should fail if the maximum value of the list is 900 or larger.

This list should specifically be generated using monadic combinators (bind) or some equivalent,
and this is a test that is only interesting for integrated shrinking.
This is only interesting as a test of a problem
[some property-based testing libraries have with monadic bind](https://clojure.github.io/test.check/growth-and-shrinking.html#unnecessary-bind).
In particular the use of the length parameter is critical,
and the challenge is to shrink this example to `[900]` reliably when using a PBT library's built in generator for lists.
  

**The Testr:**  
```csharp
Testr.Named("The maximum value of the list is smaller than 900.")
    .For(
        from length in Fuzzr.Int(1, 100)
        from list in Fuzzr.Int(0, 1000).Many(length)
        select list.ToList())
    .Assert(a => a.Max() < 900);
```

**The Report:**  
```text
------------------------------------------------------------
  The maximum value of the list is smaller than 900.
  Seed: 357470573
 ------------------------------------------------------------
  Falsified:
    Input = [ 905 ]
    Redux = [ 900 ]

  Original:
    [ 176, 244, 257, 301, 257, 329, 381, 764, 442, 261, 535, 550, 677, 905 ]
 ------------------------------------------------------------
```
### Difference

**The Fuzzr:**  
```csharp
from _ in Configr<Pair>.Construct(Fuzzr.Int(), Fuzzr.Int())
from pair in Fuzzr.One<Pair>()
select pair
```

**The Shrinkr:**  
```csharp
public class The
{
    public static readonly Shrinker Shrinkr =
        Shrink.OnType<Pair>(
            s => s.Simplify(
                Reduce.Function<Pair>(a => TowardsZero(a))));
    private static IEnumerable<Pair> TowardsZero(Pair input)
    {
        var local = input;
        while (local.A >= 0)
        {
            local = local with { A = local.A - 1, B = local.B - 1 };
            yield return local;
        }
    }
}
```
#### Test1

Test 1 ('difference must not be zero') only succeeds if 
- the first parameter is less than 10 
- _or_ the difference is not zero.
The smallest falsified sample is `[10, 10]`
  

**The Testr:**  
```csharp
Testr.Named("Difference must not be zero.")
    .For(TheFuzzr, Shrinkr)
    .Assert(a => a.A < 10 || a.A != a.B);
```

**The Report:**  
```text
------------------------------------------------------------
  Difference must not be zero.
  Seed: 1485535450
 ------------------------------------------------------------
  Falsified:
    Input = { A: 93, B: 93 }
    Redux = { A: 10, B: 10 }

  Original:
    { A: 93, B: 93 }
 ------------------------------------------------------------
```
#### Test2

Test 2 ('difference must not be small') only succeeds if 
- the first parameter is less than 10 
- _or_ the difference is not between 1 and 4.
The smallest falsified sample is `[10, 6]`.
  

**The Testr:**  
```csharp
Testr.Named("Difference must not be small.")
    .For(TheFuzzr, Shrinkr)
    .Assert(a => a.A < 10 || Math.Abs(a.A - a.B) > 4 || a.A == a.B);
```

**The Report:**  
```text
------------------------------------------------------------
  Difference must not be small.
  Seed: 1449083695
 ------------------------------------------------------------
  Falsified:
    Input = { A: 37, B: 38 }
    Redux = { A: 10, B: 6 }

  Original:
    { A: 37, B: 38 }
 ------------------------------------------------------------
```
#### Test3

Test 3 ('difference must not be one') only succeeds if 
- the first parameter is less than 10 
- _or_ the difference is not exactly 1.
The smallest falsified sample is `[10, 9]`.
  

**The Testr:**  
```csharp
Testr.Named("Difference is not exactly 1.")
    .For(TheFuzzr, Shrinkr)
    .Assert(a => a.A < 10 || Math.Abs(a.A - a.B) != 1);
```

**The Report:**  
```text
------------------------------------------------------------
  Difference is not exactly 1.
  Seed: 1231462692
 ------------------------------------------------------------
  Falsified:
    Input = { A: 51, B: 52 }
    Redux = { A: 10, B: 9 }

  Original:
    { A: 51, B: 52 }
 ------------------------------------------------------------
```
### Bin Heap

This is based on an example from QuickCheck's test suite (via the SmartCheck paper). 
It generates binary heaps, and then uses a wrong implementation of a function 
that converts the binary heap to a sorted list and asserts that the result is sorted.

Interestingly most libraries seem to never find the smallest example here, 
which is the four valued heap (0, None, (0, (0, None, None), (1, None, None))). 
This is essentially because small examples are "too sparse", so it's very hard to find one by luck.
  

**The Testr:**  
```csharp
Testr.Named("Heap sort is correct.")
    .For(TheFuzzr)
    .Deliberate(a => Flatten(a).Count())
    .Assert(a =>
    {
        var correct = Flatten(a).OrderBy(x => x).ToList();
        var buggy = Flatten(a).ToList();
        return correct.SequenceEqual(buggy);
    });
```

**The Fuzzr:**  
```csharp
from depth in Configr<Heap>.Depth(3, 20)
from inheritance in Configr<Heap>.AsOneOf(typeof(Empty), typeof(Node))
from terminator in Configr<Heap>.EndOn<Empty>()
from ctor in Configr<Node>.Construct(Fuzzr.Int(), Fuzzr.One<Heap>(), Fuzzr.One<Heap>())
from value in Configr<Node>.Property(a => a.Value,
    from cnt in Fuzzr.Counter("heap") select 1000 - cnt)
from heap in Fuzzr.One<Heap>()
select heap
```

**The Report:**  
```text
------------------------------------------------------------
  Heap sort is correct.
  Seed: 2136249593
 ------------------------------------------------------------
  Falsified:
    Input = { Left: { Value: 999, Left: { }, Right: { } }, Right: { Value: 998, Left: { }, Right: { } } }
    Redux = { Left: { Value: 999, Left: { }, Right: { } }, Right: { Value: 0, Left: { }, Right: { } } }

  Original:
    { Value: 997, Left: { Value: 999, Left: { }, Right: { } }, Right: { Value: 998, Left: { }, Right: { } } }
 ------------------------------------------------------------
```
### Coupling

In this example the elements of a list of integers are coupled to their position in an unusual way.

The expected smallest falsified sample is [1, 0].
  

**The Testr:**  
```csharp
Testr
    .Named("No two different indexes point to each other.")
    .For(
        from length in Fuzzr.Int()
        from enumerable in Fuzzr.Int(0, length - 1).Many(length)
        select enumerable.ToList())
    .DisableValueReduction()
    .Deliberate(a => a.Count, 2)
    .Assert(list => list.All(
        element =>
        {
            int index = list[element];
            if (index != element && list[index] == element)
                return false;
            return true;
        }));
```

**The Report:**  
```text
------------------------------------------------------------
  No two different indexes point to each other.
  Seed: 1934478623
 ------------------------------------------------------------
  Falsified:
    Input = [ 1, 0 ]


  Original:
    [ 1, 1, 0, 4, 3, 3, 2, 3, 11, 6, 4, 4, 4, 12 ]
 ------------------------------------------------------------
```
### Deletion

This tests the property "if we remove an element from a list, the element is
no longer in the list". The remove function we use however only actually
removes the _first_ instance of the element, so this fails whenever the list
contains a duplicate and we try to remove one of those elements.

This example is interesting for a couple of reasons:

1. It's a nice easy to explain example of property-based testing.
2. Shrinking duplicates simultaneously is something that most property-based
   testing libraries can't do.

The expected smallest falsified sample is `([0, 0], 0)`.
  

**The Testr:**  
```csharp
Testr
    .Named("Element is no longer in the list.")
    .For(Fuzzr.Tuple(Fuzzr.Int(1, 10).Many(3, 20), Fuzzr.Int(1, 10)))
    .Assert(a =>
    {
        var list = a.Item1.ToList();
        list.Remove(a.Item2);
        return !list.Contains(a.Item2);
    });
```

**The Report:**  
```text
------------------------------------------------------------
  Element is no longer in the list.
  Seed: 712389878
 ------------------------------------------------------------
  Falsified:
    Input = ( [ 1, 1 ], 1 )


  Original:
    ( [ 1, 1, 8, 3, 5, 1, 9, 1, 7, 2, 8, 6, 3 ], 1 )
 ------------------------------------------------------------
```
### Distinct

This tests the example provided for the property "a list of integers containing at least three distinct elements".

This is interesting because:

1. Most property-based testing libraries will not successfully normalize (i.e. always return the same answer) this property,
because it requires reordering examples to do so.
2. Hypothesis and test.check both provide a built in generator for "a list of distinct elements",
so the "example of size at least N" provides a sort of lower bound for how well they can shrink those built in generators.

The expected smallest falsified sample is `[0, 1, -1]` or `[0, 1, 2]`.
  

**The Testr:**  
```csharp
Testr
    .Named("Contains at least three distinct elements.")
    .For(Fuzzr.Int(1, 10).Many(3, 20))
    .Assert(a =>
    {
        if (a.Count() >= 3)
            return a.ToHashSet().Count >= 3;
        return true;
    });
```

**The Report:**  
```text
------------------------------------------------------------
  Contains at least three distinct elements.
  Seed: 1943621438
 ------------------------------------------------------------
  Falsified:
    Input = [ 3, _, 3 ]


  Original:
    [ 3, 9, 3, 9, 3 ]
 ------------------------------------------------------------
```
### Nested Lists

**The Testr:**  
```csharp
Testr
    .Named("The sum of lengths of the element lists is at most 10.")
    .For(Fuzzr.Int().Many(0, 20).ToList().Many(0, 20).ToList())
    .Deliberate(a => a.Count)
    .Assert(a => a.Sum(a => a.Count) <= 10);
```

**The Report:**  
```text
------------------------------------------------------------
  The sum of lengths of the element lists is at most 10.
  Seed: 1959968277
 ------------------------------------------------------------
  Falsified:
    Input = [ [ _, _, _, _, _, _, _, _, _, _, _ ] ]


  Original:
    [ [ 11, 46, 66 ], [ 48, 3, 13, 76, 75, 42, 97, 16, 59, 9, 29, 78, 17, 52, 68, 23, 94, 22, 8, 34 ], [ 87, 40, 45, 37, 74, 94, 74, 84, 54, 40, 38, 27, 59 ], [ 2, 23, 80, 49 ], [ 93, 39, 12, 70, 49, 30 ], [ 21, 84, 41, 23, 89, 38, 32, 36, 61, 62, 68, 39, 2, 47, 23 ], [ 6, 69, 91, 51, 58, 30 ], [ 1, 14, 72, 76, 41, 25, 75, 70, 96, 7, 99, 74, 89, 44 ], [ 50, 99, 94, 23, 61, 39 ], [ 41, 48, 84, 70, 8, 7, 1, 57, 78, 17, 90 ], [ 41, 10, 18 ], [ 91, 12, 85, 92, 72, 32, 2, 73, 33, 87, 74, 20 ], [ 56, 35, 69, 28, 45, 72, 24, 92, 70, 17, 69, 69, 39, 29, 83, 84, 72, 4, 22 ] ]
 ------------------------------------------------------------
```
  
### Web APIs
This is an example from an external lib : 'QuickWebr'.  
# Api
## Main Api Test
```csharp
Webr.Named("WebrTest")
    .Context(() => new CustomWebApplicationFactory())
    .Client(a => a.CreateClient())
    .Authentication(a => a.HasBearerToken(), a => a.AuthenticateViaTokenEndpointAsync())
    .Database(a => a.GetDbContext())
    .DisableWarnings()
    .PoolOf<CoachInfo>()
    .PoolOf<CourseInfo>()
    .Methods(
        new CreateCoach(),
        new UpdateCoachSkills(),
        new CreateCourse(),
        new UpdateCourseSkills(),
        new UpdateTimeSlots(),
        new ConfirmCourse(),
        new AssignCoachToCourse())
    .ForAll<CoachInfo>("All Assigned Coaches Must Be Suitable",
        (api, coachInfo) =>
        {
            var coach = api.Query(db => db.Find<Coach>(Id<Coach>.From(coachInfo.Id))!);
            return coach.AssignedCourses.All(course => coach.IsSuitableFor(course));
        })
    .Run(20.Runs(), 50.ExecutionsPerRun());
```
## Coach Methods
```csharp
public class CreateCoach : ApiMethod
{
    public override CheckrOf<(Func<bool>, CheckrOf<Case>)> Call(IApi api) =>
        api.Create("/coaches")
            .With(
                from name in Fuzzr.String()
                from email in Fuzzr.String()
                select new RegisterCoachRequest(name, email))
            .Returns<int>(a => a > 0)
            .When<CoachInfo>(a => a.Count <= 3)
            .Store(a => new CoachInfo(a))
            .Load((db, info) => db.Find<Coach>(Id<Coach>.From(info.Id)))
            .Expect(
                ("Name", (request, coach) => coach.Name.Value == request.Name),
                ("Email", (request, coach) => coach.Email.Value == request.Email));
}
```
```csharp
public class UpdateCoachSkills : ApiMethod
{
    public override CheckrOf<(Func<bool>, CheckrOf<Case>)> Call(IApi api) =>
        api.Update("/coaches/{id}/skills")
            .When<CoachInfo>(a => a.Count != 0)
            .With(
                from skills in Fuzzr.OneOf(Skills.Pool).Unique(Guid.NewGuid()).Many(3, 5)
                select new UpdateSkillsRequest([.. skills])
                , coach => Shrink.ValueFromState(coach.Skills, []))
            .Route(a => a.Id, a => $"/coaches/{a}/skills")
            .Store((coach, request) => coach with { Skills = request.Skills })
            .Load((db, id) => db.Find<Coach>(Id<Coach>.From(id)))
            .Expect(
                ("Skills", (request, coach) => Skills.Equal(request.Skills, coach.Skills)));
}
```
## Course Methods
```csharp
public class CreateCourse : ApiMethod
{
    public override CheckrOf<(Func<bool>, CheckrOf<Case>)> Call(IApi api) =>
        api.Create("/courses")
            .With(
                from name in Fuzzr.String()
                from startDate in Fuzzr.DateOnly(1.January(2026), 31.December(2026))
                from endDate in Fuzzr.DateOnly(startDate, startDate.AddDays(50))
                select new CreateCourseRequest(name, startDate, endDate))
            .Returns<int>(a => a > 0)
            .When<CourseInfo>(a => a.Count < 3)
            .Store(a => new CourseInfo(a))
            .Load((db, info) => db.Find<Course>(Id<Course>.From(info.Id)))
            .Expect(
                ("Name", (request, course) => course.Name.Value == request.Name),
                ("Start Date", (request, course) => course.Period.Start == request.StartDate),
                ("End Date", (request, course) => course.Period.End == request.EndDate));
}
```
```csharp
public class UpdateCourseSkills : ApiMethod
{
    public override CheckrOf<(Func<bool>, CheckrOf<Case>)> Call(IApi api) =>
        api.Update("/courses/{id}/skills")
            .When<CourseInfo>(info => !info.IsConfirmed)
            .With(
                from skills in Fuzzr.OneOf(Skills.Pool).Unique(Guid.NewGuid()).Many(3, 5)
                select skills.ToList())
            .Route(a => a.Id, a => $"/courses/{a}/skills")
            .Load((db, id) => db.Find<Course>(Id<Course>.From(id)))
            .Expect(("Skills", (request, course) => Skills.Equal(request, course.RequiredSkills)));
}
```
```csharp
public class UpdateTimeSlots : ApiMethod
{
    public override CheckrOf<(Func<bool>, CheckrOf<Case>)> Call(IApi api) =>
        api.Update("/courses/{id}/timeslots")
            .When<CourseInfo>(info => !info.IsConfirmed)
            .With(TimeSlotsFuzzr, Try.Values(new TimeSlotRequest(CourseDay.Monday, 10, 11)))
            .Route(a => a.Id, a => $"/courses/{a}/timeslots")
            .Store((course, request) => course with { HasTimeSlots = true })
            .Load((db, id) => db.Find<Course>(Id<Course>.From(id))!)
            .Expect(("Timeslots", (request, course) => SlotsAreEqual(request, course.TimeSlots)));
    private static readonly FuzzrOf<List<TimeSlotRequest>> TimeSlotsFuzzr =
        from key in Fuzzr.Guid()
        from slots in TimeSlotFuzzr(key).Many(2, 5)
        select slots.ToList();
    private static FuzzrOf<TimeSlotRequest> TimeSlotFuzzr(object key) =>
        from day in Fuzzr.Enum<CourseDay>().Unique(key)
        from start in Fuzzr.Int(9, 17)
        from end in Fuzzr.Int(start + 1, 18)
        select new TimeSlotRequest(day, start, end);
    private static bool SlotsAreEqual(IEnumerable<TimeSlotRequest> expected, IEnumerable<TimeSlot> actual)
    {
        var expectedOrdered = expected.OrderBy(a => (a.Day, a.Start, a.End));
        var actualOrdered =
            actual.Select(a => new TimeSlotRequest(a.Day, a.Start.Value, a.End.Value))
                .OrderBy(a => (a.Day, a.Start, a.End));
        return expectedOrdered.SequenceEqual(actualOrdered);
    }
}
```
```csharp
public class ConfirmCourse : ApiMethod
{
    public override CheckrOf<(Func<bool>, CheckrOf<Case>)> Call(IApi api) =>
        api.Update("/courses/{id}/confirm")
            .When<CourseInfo>(info => info.HasTimeSlots && !info.IsConfirmed)
            .Route(a => a.Id, a => $"/courses/{a}/confirm")
            .Store((course) => course with { IsConfirmed = true })
            .Load((db, id) => db.Find<Course>(Id<Course>.From(id)))
            .Expect(("Confirmed", (course) => course.IsConfirmed));
}
```
```csharp
public class AssignCoachToCourse : ApiMethod
{
    public override CheckrOf<(Func<bool>, CheckrOf<Case>)> Call(IApi api) =>
        api.Update("/courses/{id}/assign-coach")
            .When<CourseInfo, CoachInfo>(
                (course, coach) => course.IsConfirmed && IsSuitableFor(api, course, coach))
            .With((course, coach) => new AssignCoachRequest(coach.Id))
            .Route((course, coach) => course.Id, a => $"/courses/{a}/assign-coach")
            .Load((ctx, course, coach) => (ctx.Find<Course>(Id<Course>.From(course.Id)), ctx.Find<Coach>(Id<Coach>.From(coach.Id))))
            .Expect(
                ("Coach is assigned", (course, coach) =>
                    course.AssignedCoach == coach &&
                    coach.AssignedCourses.Contains(course)));
    private static bool IsSuitableFor(IDbAccess db, CourseInfo courseInfo, CoachInfo coachInfo)
    {
        var course = db.Query(ctx => ctx.Find<Course>(Id<Course>.From(courseInfo.Id))!);
        if (course == null) return false;
        if (course.AssignedCoach != null) return false;
        var coach = db.Query(ctx => ctx.Find<Coach>(Id<Coach>.From(coachInfo.Id))!);
        if (coach == null) return false;
        return coach.IsSuitableFor(course);
    }
}
```
## Report
```
------------------------------------------------------------
 Original failing run:    32 executions
 Minimal failing case:    8 executions (after 39 shrinks)
 Seed:                    1854160474
 ------------------------------------------------------------
  Executed: Auth, Route: /courses
   -  Pool: '/courses' to Pool = CourseInfo-1
   - Input: '/courses' Request = { Name: "nxlarowge", StartDate: 15.March(2026), EndDate: 23.April(2026) }
 ------------------------------------------------------------
  Executed: Route: /coaches
   -  Pool: '/coaches' to Pool = CoachInfo-1
 ------------------------------------------------------------
  Executed: Route: /courses/{id}/skills
   -  Pool: /courses/{id}/skills           = CourseInfo-1
   - Input: '/courses/{id}/skills' Request = [ "JavaScript" ]
 ------------------------------------------------------------
  Executed: Route: /courses/{id}/timeslots
   -  Pool: /courses/{id}/timeslots           = CourseInfo-1
   - Input: '/courses/{id}/timeslots' Request = [ _ ]
 ------------------------------------------------------------
  Executed: Route: /courses/{id}/confirm
   -  Pool: /courses/{id}/confirm = CourseInfo-1
 ------------------------------------------------------------
  Executed: Route: /coaches/{id}/skills
   -  Pool: /coaches/{id}/skills           = CoachInfo-1
   - Input: '/coaches/{id}/skills' Request = { Skills: [ "JavaScript" ] }
 ------------------------------------------------------------
  Executed: Route: /courses/{id}/assign-coach
   -  Pool: /courses/{id}/assign-coach from Pool = ( CourseInfo-1, CoachInfo-1 )
 ------------------------------------------------------------
  Executed: Route: /coaches/{id}/skills
   -  Pool: /coaches/{id}/skills           = CoachInfo-1
   - Input: '/coaches/{id}/skills' Request = { Skills: [ "C#", "React", "CI/CD", "DomainDrivenDesign", "EF Core" ] }
            Redux                            { Skills: [ ] }
 ================================================================
  !! Expectation Failed: All Assigned Coaches Must Be Suitable
 ================================================================
 Passed Expectations
 - '/courses' Status Code is Success: 3x
 - '/courses' Response: 3x
 - '/courses' Name: 3x
 - '/courses' Start Date: 3x
 - '/courses' End Date: 3x
 - All Assigned Coaches Must Be Suitable: 31x
 - '/coaches' Status Code is Success: 4x
 - '/coaches' Response: 4x
 - '/coaches' Name: 4x
 - '/coaches' Email: 4x
 - '/courses/{id}/timeslots' Status Code is Success: 6x
 - '/courses/{id}/timeslots' Timeslots: 6x
 - '/courses/{id}/skills' Status Code is Success: 3x
 - '/courses/{id}/skills' Skills: 3x
 - '/coaches/{id}/skills' Status Code is Success: 11x
 - '/coaches/{id}/skills' Skills: 11x
 - '/courses/{id}/confirm' Status Code is Success: 3x
 - '/courses/{id}/confirm' Confirmed: 3x
 - '/courses/{id}/assign-coach' Status Code is Success: 2x
 - '/courses/{id}/assign-coach' Coach is assigned: 2x
 ------------------------------------------------------------
```
  
## On Shrinking
### Philosophy

QuickCheckr keeps shrinking simple and predictable.  
It never tries to *outsmart* your domain with clever global algorithms.  
Shrinking is purely local and irrelevance-based: if removing a value (or execution) still makes the property fail,
QuickCheckr marks it as *irrelevant*.  
This avoids surprising or *magical* shrink results.  
### Contents

- [Execution Shrinking][ExecutionShrinking]
- [Input Shrinking][InputShrinking]
- [Input Reduction][InputReduction]
- [Deliberation Shrinking][DeliberationShrinking]
  

[ExecutionShrinking]: #execution-shrinking

[InputShrinking]: #input-shrinking

[InputReduction]: #input-reduction

[DeliberationShrinking]: #deliberation-shrinking
### Execution Shrinking
[On Shrinking][OnShrinking] | [Next][InputShrinking]  
  
This is the first step in QuickCheckr shrinking.  
The following simple example fails on the third execution.  
```csharp
from counter in Checkr.Input("counter", Fuzzr.Counter("counter"))
from expect in Checkr.Expect("Counter does not equal 3", () => counter != 3)
select Case.Closed;
```

Execution 1: counter = 1  
Execution 2: counter = 2  
Execution 3: counter = 3  (Fails)  

During execution shrinking the values from counter are not regenerated, but retrieved from the previous run.
So the ExecutionShrinker retries the run with the same values, but tries to remove executions, one by one.  
If the run still fails, the execution is not deemed relevant for the failure and is removed.  

So after *Execution Shrinking*, the result is:
  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                ExecutionShrinking.cs:68:1
 Original failing run:    3 executions
 Minimal failing case:    1 execution (after 2 shrinks)
 Seed:                    669391953
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

[OnShrinking]: #on-shrinking

[InputShrinking]: #input-shrinking
### Input Shrinking
[On Shrinking][OnShrinking] | [Previous][ExecutionShrinking] | [Next][InputReduction]  
  
This is the next step in QuickCheckr shrinking and the most complex one.  

In QuickCheckr, **input shrinking is a two-step process**:

1. **Classification**: decide *what is relevant* for reproducing the failure.
2. **Reduction (Redux)**: simplify the *relevant* parts to improve human understanding, while still reproducing the failure.

A key principle: the engine always keeps the **original failing input**.
Reduction produces a **Redux view** alongside the original.
The report can therefore show both **Original** and **Redux** without pretending the original counterexample was replaced.  
Reduction is (by default) an opt-in feature. In this chapter we are mainly demonstrating Classification.
Reduction will be elaborated on later.
  
The following simple example has one input but always fails.  
```csharp
from irrelevant in Checkr.Input("Irrelevant Input", Fuzzr.Int())
from expect in Checkr.Expect("Always Fails", () => false)
select Case.Closed;
```
Input shrinking reruns the failing execution and prunes inputs whose values don't affect the failure.

So, in the previous example, after *Input Shrinking*, the result is:
  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Docs\H_OnShrinking\C_InputShrinking\InputShrinking.cs:39:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    165641598
 ------------------------------------------------------------
  Executed:
   - WARNING: All inputs were considered irrelevant.
 =======================================
  !! Expectation Failed: Always Fails
 =======================================
```
Another example, slightly trickier.  
```csharp
from one in Checkr.Input("Input One", Fuzzr.Int())
from two in Checkr.Input("Input Two", Fuzzr.Int())
from three in Checkr.Input("Input Three", Fuzzr.Int())
from result in Checkr.Act("Result", () => one + three)
from expect in Checkr.Expect("Should not be 42", () => result != 42)
select Case.Closed;
```
As you can see from the test definition, two inputs are involved in a possible failure.

QuickCheckr correctly identifies the relevant inputs.

Result:
  
```text
------------------------------------------------------------
 Test:                    Example_Two
 Location:                Docs\H_OnShrinking\C_InputShrinking\InputShrinking.cs:81:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    231122398
 ------------------------------------------------------------
  Executed: Result
   - Input: Input One   = 12
   - Input: Input Three = 30
 ===========================================
  !! Expectation Failed: Should not be 42
 ===========================================
```
Many more examples focused on shrinking behaviour can be found [here][HoneyIShrunkTheInputs].  

[HoneyIShrunkTheInputs]: #honey-i-shrunk-the-inputs

[OnShrinking]: #on-shrinking

[ExecutionShrinking]: #execution-shrinking

[InputReduction]: #input-reduction
### Input Reduction
[On Shrinking][OnShrinking] | [Previous][InputShrinking] | [Next][DeliberationShrinking]  
  
After InputShrinking has classified an input as either relevant or irrelevant, any registered *reducers*
are run on a relevant input (example below).  

Note the use of the word *registered*. Staying true to it's shrinking philosophy,
QuickChecker does not try and guess how to reduce input values. It's up to the user to supply that information.  

Most of the time, even in the example here I think, reducers are not needed to figure out what went wrong from the report.  
But in some cases it is a must.  

So what is a reducer ?  

Well let me show you:  

**The Model:**  
```csharp
public class Account
{
    public int Balance { get; private set; }
    public void Deposit(int amount) => Balance += amount;
    public void WithDraw(int amount) => Balance -= amount;
}
```

**The Checkr:**  
We're running the same checkr, with the same seed.
The only difference is the addition of `Reduce.TowardsZero` in the `Checkr.Input` argument list.  
```csharp
from account in Trackr.Stashed(() => new Account())
from amount in Checkr.Input("Amount", Fuzzr.Int(),
    Shrink.OnType<int>(s => s.Simplify(Reduce.TowardsZero)))
from operations in Checkr.OneOf(
    Checkr.Act("Deposit", () => account.Deposit(amount)),
    Checkr.Act("Withdraw", () => account.WithDraw(amount)))
from noOverdraft in Checkr.Expect("No Overdraft", () => account.Balance >= 0)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                InputReduction.cs:68:1
 Original failing run:    2 executions
 Minimal failing case:    1 execution (after 2 shrinks)
 Seed:                    1684147193
 ------------------------------------------------------------
  Executed: Withdraw
   - Input: Amount = 87
            Redux    1
 =======================================
  !! Expectation Failed: No Overdraft
 =======================================
 Passed Expectations
 - No Overdraft: 1x
 ------------------------------------------------------------
```
And as you can see it does exactly what it says on the tin. 
It *reduces* the original value using a user supplied algorithm.  
This one just decrements the input until (and including) it reaches zero, and checks if the run still fails.
If the run passes, the value last tried is reported as the *reduced* input.  

In the example here, `0` was tried, but it made the test pass, so the previously tried candidate `1` is reported.  

QuickCheckr comes with a small number of built-in reducers, but it is relatively easy to implement your own.  
Depending on demand, the library of reducers might grow fast.
  

[OnShrinking]: #on-shrinking

[InputShrinking]: #input-shrinking

[DeliberationShrinking]: #deliberation-shrinking
### Deliberation Shrinking
[On Shrinking][OnShrinking] | [Previous][InputReduction]  
  
This next step, which is disabled by default, allows the shrinker to find *global* minimal failcases instead of *local* ones.  

A first example, as well as how to enable this feature,
can be found in the entry on Deliberation Policies in the [Configuration][Configuration] chapter.

But here are some more use cases.  
#### An Eager Shrinker
Under some rare circumstances, the Shrinker gets *greedy*.  
One currently identified case where this happens is if more than one input triggers the same failure.  
```csharp
from input in Checkr.Input("Input", Fuzzr.Int(0, 5).Many(3))
from trace in Checkr.Trace("List", () => input)
from expect in Checkr.ExpectWhen("List count > 2 and does not contain 1",
    () => input.Count() > 2,
    () => !input.Contains(1))
select Case.Closed;
```
Now when the list contains multiple elements `1`, we might get:  
```text
------------------------------------------------------------
 Test:                    RunTest_DeliberatedGreedy
 Location:                Docs\H_OnShrinking\F_DeliberationShrinking\DeliberationShrinking.cs:45:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 3 shrinks)
 Seed:                    227109555
 ------------------------------------------------------------
  Executed:
   - Input: Input = [ _, _, _ ]
   - Trace: List  = [ 1, 2, 1 ]
 ================================================================
  !! Expectation Failed: List count > 2 and does not contain 1
 ================================================================
```
It seems QuickCheckr thinks that the minimal fail-case is a list with atleast 3 elements (*correct*),
but the element values are considered irrelevant (*wrong*).  
For this rare case we  add a DeliberationPolicy, allowing a more explanatory fail-case to surface:  
```csharp
private static CheckrConfig ConfigureShrinkCount(CheckrConfig config)
    => config with { DeliberationPolicy = DeliberationPolicy.MinimumShrinkCount };
```
**Report:**  
```text
------------------------------------------------------------
 Test:                    RunTest_DeliberatedGreedyFixed
 Location:                Docs\H_OnShrinking\F_DeliberationShrinking\DeliberationShrinking.cs:82:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 2 shrinks)
 Seed:                    1133636239
 ------------------------------------------------------------
  Executed:
   - Input: Input = [ _, _, 1 ]
   - Trace: List  = [ 0, 4, 1 ]
 =============================================
  !! Expectation Failed: Does not contain 1
 =============================================
```
A similar thing can happen when shrinking objects, and the solution is exactly the same  
```csharp
from obj in Checkr.Input("BaggyInters",
    from cfg in Configr.Primitive(Fuzzr.Int(1, 5))
    from result in Fuzzr.One<BaggyInters>()
    select result)
from trace in Checkr.Trace("Original", () => obj)
from expect in Checkr.Expect("obj.IntOne == 1 || obj.IntThree == 1",
    () => obj.IntOne != 1 && obj.IntThree != 1)
select Case.Closed;
```
**With Deliberation Policy:**  
```text
------------------------------------------------------------
 Test:                    RunTest_DeliberatedGreedyObjects
 Location:                Docs\H_OnShrinking\F_DeliberationShrinking\DeliberationShrinking.cs:105:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 2 shrinks)
 Seed:                    149961996
 ------------------------------------------------------------
  Executed:
   - Input: BaggyInters = { IntThree: 1 }
   - Trace: Original    = { IntOne: 3, IntTwo: 2, IntThree: 1 }
 ===============================================================
  !! Expectation Failed: obj.IntOne == 1 || obj.IntThree == 1
 ===============================================================
```

[Configuration]: #configuration

[OnShrinking]: #on-shrinking

[InputReduction]: #input-reduction
## You Can Go Your Own Way
We already saw in the chapter on [Configuration][Configuration] how you can turn some knobs to customize QuickCheckr's behaviour
and/or reporting globally.  
But there are ways to obtain more fine-grained control. That is what this chapter is about.  
### Contents

- [Custom Shrinkers][CustomShrinkers]
- [Showr][A_TheShowrClass]
  

[Configuration]: #configuration

[A_TheShowrClass]: #the-showr-class

[CustomShrinkers]: #custom-shrinkers
### Custom Shrinkers
As explained in [OnShrinking][InputShrinking], a Shrinker is comprised of:
  1. A *[Classifier][Classifiers]* : responsible for finding the relevant inputs.
  2. A *[Reducer][Reducers]* : responsible for finding a minimal value.

QuickCheckr allows you to define either one or both of these on a per-input basis.  
This gives you precise control over how shrinking behaves when the default strategies are insufficient for your domain.  

By default, built-in classifiers and reducers are provided for common types.  
Custom shrinkers are intended for cases where domain knowledge can guide shrinking more effectively. 

They are registered on a per-input basis using an overload on the `Checkr.Input)()` method.  
Contruction of a shrinker is done using factory methods on the `Shrink` static class.

Let's have a look at ...  
#### A Simple Example

**The Checkr:**  
```csharp
from answers in Checkr.Input("Answer", Fuzzr.Int(),
    Shrink.OnType<int>(
        s => s.Classify(Try.Values(42)),
        s => s.Simplify(Reduce.TowardsZero)))
from expect in Checkr.Expect("Answer equals 42", () => answers == 42)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                A_FirstShrinker.cs:32:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    1895340861
 ------------------------------------------------------------
  Executed:
   - Input: Answer = 53
            Redux    43
 ===========================================
  !! Expectation Failed: Answer equals 42
 ===========================================
```
##### Explaining the Example, TODO: REVIEW THIS TEST/DOC
The example above is intentionally simple. The expectation only passes for the value `42`,
which makes it easy to see how the custom shrinker influences the reported case.

QuickCheckr already provides built-in classifiers and reducers for common types such as `int`.
These default strategies work well in most situations, but they are necessarily generic: 
they do not know which values are meaningful in the context of a specific check.

By adding:

```csharp
s => s.Classify(Try.Values(42))
```

we tell QuickCheckr that `42` is a value worth trying explicitly during shrinking.

The reducer then continues simplifying the remaining failing value:

```csharp
s => s.Simplify(Reduce.TowardsZero)
```

In the report, the original failing value is `53`, while the final reported value becomes `43`.
This is useful because `43` is the closest failing value above the only passing value (`42`).

Although the expectation itself is artificial, the example illustrates the purpose of custom shrinkers:
they allow shrinking to be guided by values that are semantically meaningful for the domain, not just structurally smaller.
  
#### A Second Example

**The Checkr:**  
```csharp
from answers in Checkr.Input("answers", Fuzzr.Int().Many(1, 5),
    Shrink.OnType<int>(
        s => s.Classify(Try.Values(42)),
        s => s.Simplify(Reduce.Towards(0))))
from trace in Checkr.Trace("original", () => answers)
from expect in Checkr.Expect("all answers equal 42", () => answers.All(a => a == 42))
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                B_SecondShrinker.cs:32:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    960262083
 ------------------------------------------------------------
  Executed:
   - Input: answers  = [ 73 ]
            Redux      [ 43 ]
   - Trace: original = [ 73 ]
 ===============================================
  !! Expectation Failed: all answers equal 42
 ===============================================
```

[InputShrinking]: #input-shrinking

[Classifiers]: #classifiers

[Reducers]: #reducers
#### Classifiers
Classification is done by trying different *candidates* for an input and verifying if the test still fails.  

- If removing/changing the input (via candidates) still reproduces the failure, the input is irrelevant.
- If the failure goes away for at least one candidate, the input is relevant.

So when supplying a custom classifier, you are basically supplying a list of candidates.  
There are various ways to do this :
- [Candidate-based Classifiers][CandidateClassifiers]
- [Policy-based Classifiers][PolicyBasedClassifiers]
  
The following example demonstrates how the various classifiers can be used chained.    
It is a bit of a contrived example as the default classifiers handle this case just fine.  

**The Checkr:**  
```csharp
from answer in Checkr.Input("answer", Fuzzr.Int(),
    Shrink.OnType<int>(s => s.Classify(Try.Default<int>(), Try.Values(0, 1, 42))))
from expect in Checkr.Expect("answer == 42", () => answer == 42)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Classifiers.cs:37:1
 Original failing run:    1 execution
 Seed:                    1521363987
 ------------------------------------------------------------
  Executed:
   - Input: answer = 63
 =======================================
  !! Expectation Failed: answer == 42
 =======================================
```

[CandidateClassifiers]: #candidate-based-classifiers

[PolicyBasedClassifiers]: #policy-based-classifiers
##### Candidate-based Classifiers
Here is an overview of all the different ways you can define *candidates* for classification.  
Using a more relevant example this time (where possible), that allows you to deal with [this][E_CheckingNulls] relatively common issue.  
###### Values

**The Checkr:**  
```csharp
from counter in Checkr.Input("counter", Fuzzr.Counter("counter"))
from thing in Checkr.Input("Null Thing",
    Fuzzr.Constant<Thing>(null!),
    Shrink.OnType<Thing>(
        s => s.Classify(Try.Values(new Thing()))))
from expect in Checkr.Expect("counter != 2", () => counter != 2)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                A_TryValues.cs:73:1
 Original failing run:    2 executions
 Minimal failing case:    1 execution (after 2 shrinks)
 Seed:                    161541321
 ------------------------------------------------------------
  Executed:
   - Input: counter = 2
 =======================================
  !! Expectation Failed: counter != 2
 =======================================
 Passed Expectations
 - counter != 2: 1x
 ------------------------------------------------------------
```
###### Collection
This is very similar to `Try.Values`.  
It only exists to avoid generic type soup and easily made mistakes when typing candidates supply for collections.  

**The Checkr:**  
```csharp
from answers in Checkr.Input("answers", Fuzzr.Int().Many(2, 5),
    Shrink.OnType<IEnumerable<int>>(s => s.Classify(Try.Collection([0], [1], [42]))))
from expect in Checkr.Expect("all answers equal 42", () => answers.All(a => a == 42))
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                B_TryCollection.cs:56:1
 Original failing run:    1 execution
 Seed:                    899182715
 ------------------------------------------------------------
  Executed:
   - Input: answers = [ 2, 80 ]
 ===============================================
  !! Expectation Failed: all answers equal 42
 ===============================================
```
###### Function
This method allows you to supply dynamic candidates using a function.  
The lambda argument of the supplied function, is the original Input value.
  

**The Checkr:**  
```csharp
from counter in Checkr.Input("counter", Fuzzr.Counter("counter"))
from thing in Checkr.Input("Null Thing",
    Fuzzr.Constant<Thing>(null!),
    Shrink.OnType<Thing>(s => s.Classify(Try.Function((Thing a) => [new Thing()]))))
from expect in Checkr.Expect("counter != 2", () => counter != 2)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                C_TryFunction.cs:73:1
 Original failing run:    2 executions
 Minimal failing case:    1 execution (after 2 shrinks)
 Seed:                    161541321
 ------------------------------------------------------------
  Executed:
   - Input: counter = 2
 =======================================
  !! Expectation Failed: counter != 2
 =======================================
 Passed Expectations
 - counter != 2: 1x
 ------------------------------------------------------------
```
###### Fuzzr

**The Checkr:**  
```csharp
from counter in Checkr.Input("counter", Fuzzr.Counter("counter"))
from thing in Checkr.Input("Null Thing", Fuzzr.Constant<Thing>(null!),
    Shrink.OnType<Thing>(s => s.Classify(Try.Fuzzr(Fuzzr.One<Thing>(), 2))))
from expect in Checkr.Expect("counter != 2", () => counter != 2)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                D_TryFuzzr.cs:73:1
 Original failing run:    2 executions
 Minimal failing case:    1 execution (after 2 shrinks)
 Seed:                    161541321
 ------------------------------------------------------------
  Executed:
   - Input: counter = 2
 =======================================
  !! Expectation Failed: counter != 2
 =======================================
 Passed Expectations
 - counter != 2: 1x
 ------------------------------------------------------------
```

[E_CheckingNulls]: #checking-nulls
##### Policy-based Classifiers
  
###### Keep
`Try.Keep` explicitly marks an execution as relevant and worth preserving during shrinking.
In most scenarios this is not strictly necessary, as QuickCheckr will naturally retain behaviour that contributes to failure.
`Try.Keep` mainly exists for completeness and for rare performance-tuning cases where you want to prevent the shrinker from exploring removals you already know are pointless.
It also serves as an explicit signal of intent: this step is considered essential evidence.  

**The Checkr:**  
```csharp
from counter in Checkr.Input("counter", Fuzzr.Counter("counter"))
from thing in Checkr.Input("Null Thing",
    Fuzzr.Constant<Thing>(null!),
    Shrink.OnType<Thing>(s => s.Classify(Try.Keep<Thing>())))
from expect in Checkr.Expect("counter != 2", () => counter != 2)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                A_TryKeep.cs:46:1
 Original failing run:    2 executions
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    161541321
 ------------------------------------------------------------
  Executed:
   - Input: counter    = 2
   - Input: Null Thing = null
 =======================================
  !! Expectation Failed: counter != 2
 =======================================
 Passed Expectations
 - counter != 2: 1x
 ------------------------------------------------------------
```
###### Default
`Try.Keep` applies QuickCheckr's normal relevance detection as one strategy within a classifier chain.
Classifiers are executed in succession, so Try.Default can be placed anywhere in the sequence.
If it finds a relevant candidate, the chain stops. Otherwise, the next classifier is attempted.
This makes it possible to combine engine-driven relevance detection with more opinionated strategies such as Try.Values(...)
without hard-coding one as the global default.  

**The Checkr:**  
```csharp
from counter in Checkr.Input("counter", Fuzzr.Counter("counter"))
from thing in Checkr.Input("Null Thing",
    Fuzzr.Constant<Thing>(null!),
    Shrink.OnType<Thing>(s => s.Classify(Try.Default<Thing>())))
from expect in Checkr.Expect("counter != 2", () => counter != 2)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                C_TryDefault.cs:46:1
 Original failing run:    2 executions
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    161541321
 ------------------------------------------------------------
  Executed:
   - Input: counter    = 2
   - Input: Null Thing = null
   - WARNING: Null value for type 'Thing' at input 'Null Thing'.
 =======================================
  !! Expectation Failed: counter != 2
 =======================================
 Passed Expectations
 - counter != 2: 1x
 ------------------------------------------------------------
```
#### Reducers
##### Custom Reducers Old
Reducers are a relatively new addition to the QuickCheckr arsenal.  
And you could, rightfully so, point out that they violate it's core philosophy
of not rewriting what actually happened.  

But once again, it is a feature born out of necessity.  
 
With that caveat out of the way, let's dig in.  
  
###### Values Stop On First Passed

**The Checkr:**  
```csharp
from input in Checkr.Input("Input", Fuzzr.Int(), Shrink.OnType<int>(s => s.Simplify(Reduce.Values([3, 2, 1, 0]))))
from expect in Checkr.Expect("Input < 2", () => input < 2)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                A_ValuesStopOnFirstPassed.cs:59:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    1681332589
 ------------------------------------------------------------
  Executed:
   - Input: Input = 79
            Redux   2
 ====================================
  !! Expectation Failed: Input < 2
 ====================================
```
###### Values Stop On First Passed Alternative

**The Checkr:**  
```csharp
from input in Checkr.Input("Input", Fuzzr.Int(), Shrink.OnType<int>(s => s.Simplify(Reduce.Values([3, 0, 1, 2]))))
from expect in Checkr.Expect("Input < 2", () => input < 2)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                B_ValuesStopOnFirstPassedAlternative.cs:59:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    1681332589
 ------------------------------------------------------------
  Executed:
   - Input: Input = 79
            Redux   3
 ====================================
  !! Expectation Failed: Input < 2
 ====================================
```
###### Values Report Last Failure

**The Checkr:**  
```csharp
from input in Checkr.Input("Input", Fuzzr.Int(),
   Shrink.OnType<int>(s => s.Simplify(Reduce.Values([3, 0, 1, 2], true))))
from expect in Checkr.Expect("Input < 2", () => input < 2)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                C_ValuesReportLastFailure.cs:59:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    1681332589
 ------------------------------------------------------------
  Executed:
   - Input: Input = 79
            Redux   2
 ====================================
  !! Expectation Failed: Input < 2
 ====================================
```
###### Collection
Helper function in order to avoid generic type confusion.  

**The Checkr:**  
```csharp
from input in Checkr.Input("Input", Fuzzr.Int().Many(1, 5),
   Shrink.OnType<IEnumerable<int>>(s => s.Simplify(Reduce.Collection([[3], [2], [1], [0]]))))
from expect in Checkr.Expect("All < 2", () => input.All(a => a < 2))
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                D_Collection.cs:32:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 4 shrinks)
 Seed:                    1681332589
 ------------------------------------------------------------
  Executed:
   - Input: Input = [ 94 ]
            Redux   [ 2 ]
 ==================================
  !! Expectation Failed: All < 2
 ==================================
```
###### Collection Of
Helper function in order to avoid generic type confusion.  

**The Checkr:**  
```csharp
from input in Checkr.Input("Input", Fuzzr.Int().Many(1, 5),
   Shrink.OnType<IEnumerable<int>>(s => s.Simplify(Reduce.CollectionOf<int>(a => [[3], [2], [1], [0]]))))
from expect in Checkr.Expect("All < 2", () => input.All(a => a < 2))
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                D_CollectionOf.cs:59:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 4 shrinks)
 Seed:                    1681332589
 ------------------------------------------------------------
  Executed:
   - Input: Input = [ 94 ]
            Redux   [ 2 ]
 ==================================
  !! Expectation Failed: All < 2
 ==================================
```
###### Function Stop On First Passed

**The Checkr:**  
```csharp
from input in Checkr.Input("Input", Fuzzr.Int(),
   Shrink.OnType<int>(s => s.Simplify(Reduce.Function<int>(a => [3, 2, 1, 0]))))
from expect in Checkr.Expect("Input < 2", () => input < 2)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                A_FunctionStopOnFirstPassed.cs:59:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    1681332589
 ------------------------------------------------------------
  Executed:
   - Input: Input = 79
            Redux   2
 ====================================
  !! Expectation Failed: Input < 2
 ====================================
```
###### Function Stop On First Passed Alternative

**The Checkr:**  
```csharp
from input in Checkr.Input("Input", Fuzzr.Int(),
   Shrink.OnType<int>(s => s.Simplify(Reduce.Function<int>(a => [3, 0, 1, 2]))))
from expect in Checkr.Expect("Input < 2", () => input < 2)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                B_FunctionStopOnFirstPassedAlternative.cs:59:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    1681332589
 ------------------------------------------------------------
  Executed:
   - Input: Input = 79
            Redux   3
 ====================================
  !! Expectation Failed: Input < 2
 ====================================
```
###### Function Report Last Failure

**The Checkr:**  
```csharp
from input in Checkr.Input("Input", Fuzzr.Int(),
   Shrink.OnType<int>(s => s.Simplify(Reduce.Function<int>(a => [3, 0, 1, 2], true))))
from expect in Checkr.Expect("Input < 2", () => input < 2)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                C_FunctionReportLastFailure.cs:59:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    1681332589
 ------------------------------------------------------------
  Executed:
   - Input: Input = 79
            Redux   2
 ====================================
  !! Expectation Failed: Input < 2
 ====================================
```
###### Chained

**The Checkr:**  
```csharp
from input in Checkr.Input("Input", Fuzzr.Int(),
   Shrink.OnType<int>(s => s.Simplify(Reduce.Towards(5), Reduce.Towards(0))))
from expect in Checkr.Expect("Input < 2", () => input < 2)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                A_Chained.cs:32:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    1681332589
 ------------------------------------------------------------
  Executed:
   - Input: Input = 79
            Redux   2
 ====================================
  !! Expectation Failed: Input < 2
 ====================================
```
###### Reduce Towards Zero

**The Checkr:**  
```csharp
from input in Checkr.Input("Input", Fuzzr.Int(),
    Shrink.OnType<int>(s => s.Simplify(Reduce.TowardsZero)))
from expect in Checkr.Expect("Input should be smaller than 42", () => input < 42)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                A_ReduceTowardsZero.cs:59:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    1681332589
 ------------------------------------------------------------
  Executed:
   - Input: Input = 79
            Redux   42
 ==========================================================
  !! Expectation Failed: Input should be smaller than 42
 ==========================================================
```
###### Reduce Minimize String

**The Checkr:**  
```csharp
from text in Checkr.Input("Text", Fuzzr.String(),
    Shrink.OnType<string>(s => s.Simplify(Reduce.MinimizeString)))
from expect in Checkr.Expect("no a and b", () => !(text.Contains('a') && text.Contains('b')))
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                B_ReduceMinimizeString.cs:59:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    1834440365
 ------------------------------------------------------------
  Executed:
   - Input: Text  = "tdblaulb"
            Redux   "ab"
 =====================================
  !! Expectation Failed: no a and b
 =====================================
```
###### Nested In Object

**The Model:**  
```csharp
public class TheUniverse
{
    public int Answer { get; set; }
}
```

**The Checkr:**  
```csharp
from answer in Checkr.Input("Answer", Fuzzr.Int(),
    Shrink.OnType<int>(s => s.Simplify(Reduce.TowardsZero)))
from universe in Checkr.Capture(() => new TheUniverse { Answer = answer })
from notTooBig in Checkr.Expect("answer <= 42", () => universe.Answer <= 42)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                NestedInObject.cs:64:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    1930190827
 ------------------------------------------------------------
  Executed:
   - Input: Answer = 96
            Redux    43
 =======================================
  !! Expectation Failed: answer <= 42
 =======================================
```
  
  
###### Nested Reducer In Object

**The Model:**  
```csharp
public class TheUniverse
{
    public int Answer { get; set; }
}
```

**The Checkr:**  
```csharp
from universe in Checkr.Input("Universe", Fuzzr.One<TheUniverse>(), Shrink.OnType<int>(s => s.Simplify(Reduce.TowardsZero)))
from notTooBig in Checkr.Expect("answer <= 42", () => universe.Answer <= 42)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                NestedReducerInObject.cs:64:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    686899245
 ------------------------------------------------------------
  Executed:
   - Input: Universe = { Answer: 61 }
            Redux      { Answer: 43 }
 =======================================
  !! Expectation Failed: answer <= 42
 =======================================
```
  
  
###### Custom

**The Checkr:**  
```csharp
from input in Checkr.Input("Input", Fuzzr.Int(), Shrink.OnType<int>(s => s.Simplify(CustomTowardsZero.Reducer())))
from expect in Checkr.Expect("Input < 2", () => input < 2)
select Case.Closed;
```

**Reducer:**  
```csharp
public class CustomTowardsZero
{
    public static Func<Reducer<int>> Reducer() =>
        () => new CustomTowardsZero().Reduce;
    private int lastFailed = 0;
    public Reduction Reduce(ReductionResult result)
    {
        var value = result.ValueAs<int>();
        if (result.Failed)
            lastFailed = value;
        else
            return new Reduction.Stop(lastFailed);
        if (value == 0)
            return new Reduction.Stop(lastFailed);
        if (value > 0)
            value--;
        else
            value++;
        return new Reduction.Next(value);
    }
}
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                A_Custom.cs:59:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    211010750
 ------------------------------------------------------------
  Executed:
   - Input: Input = 10
            Redux   2
 ====================================
  !! Expectation Failed: Input < 2
 ====================================
```
###### Reduction Not Possible

**The Checkr:**  
```csharp
from input in Checkr.Input("Input", Fuzzr.Int(),
   Shrink.OnType<int>(s => s.Simplify(Reduce.TowardsZero)))
from expect in Checkr.Expect("Input should be greater than 42", () => input > 42)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                ReductionNotPossible.cs:32:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    1373577700
 ------------------------------------------------------------
  Executed:
   - Input: Input = 21
            Redux   0
   - WARNING: No witness for reducer at 'Input'.
 ==========================================================
  !! Expectation Failed: Input should be greater than 42
 ==========================================================
```
##### Max Steps Exceeded

**The Checkr:**  
```csharp
from input in Checkr.Input("Input", Fuzzr.Constant(42))
from expect in Checkr.Expect("Input <= 0", () => input <= 0)
select Case.Closed;
```

**The Configuration:**  
```csharp
config with { MaxStepsPerReducer = 10, ShrinkMode = config.ShrinkMode | ShrinkMode.Reduction };
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                A_MaxStepsExceeded.cs:24:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    1719639579
 ------------------------------------------------------------
  Executed:
   - Input: Input = 42
            Redux   30
   - WARNING: Reducer stopped at Input. Max steps (10) exceeded.
 =====================================
  !! Expectation Failed: Input <= 0
 =====================================
```
##### On Path

**The Checkr:**  
```csharp
from inter in Checkr.Input("Inter", Fuzzr.Constant(new Inter(42, 21)),
    Shrink.When<int>(
        s => s.As<Inter>(a => a.A).Target(),
        s => s.Simplify(Reduce.Towards(3))),
    Shrink.When<int>(
        s => s.As<Inter>(a => a.B).Target(),
        s => s.Simplify(Reduce.Towards(0))))
from expect in Checkr.Expect("Sum smaller than 11.", () => inter.A + inter.B < 11)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                OnPath.cs:58:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 2 shrinks)
 Seed:                    776489069
 ------------------------------------------------------------
  Executed:
   - Input: Inter = { A: 42, B: 21 }
            Redux   { A: 3, B: 8 }
   - WARNING: No witness for reducer at 'Inter.A'.
 ===============================================
  !! Expectation Failed: Sum smaller than 11.
 ===============================================
```
##### On Specific Value

**The Checkr:**  
```csharp
from inter in Checkr.Input("Inter", Fuzzr.Constant(new Inter(42, 5)),
    Shrink.When<int>(
        s => s.Value(a => a == 5),
        s => s.Simplify(Reduce.Towards(0))))
from trace in Checkr.Trace("Original", () => inter)
from expect in Checkr.Expect("All ints smaller than 1.", () => inter.A < 1 && inter.B < 1)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                OnSpecificValue.cs:58:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 2 shrinks)
 Seed:                    1329962298
 ------------------------------------------------------------
  Executed:
   - Input: Inter    = { B: 5 }
            Redux      { B: 1 }
   - Trace: Original = { A: 42, B: 5 }
 ===================================================
  !! Expectation Failed: All ints smaller than 1.
 ===================================================
```
### The Showr Class
This static factory class produces a CheckrOf<Case> which allows one to configure the pretty-printer used in the report.

It is implemented as such, instead of as an option on the configuration object,
because this allows for two useful things:  
> You can change configuration anywhere in the `LINQ chain`, multiple times.  

**The Checkr:**  
```csharp
from anInt in Checkr.Input("AnInt", Fuzzr.Int())
from showrOne in Showr.ForTrace(() => Please.AllowMe().ToReplace<int>(a => $"PREFIXED {a}"))
from traceOne in Checkr.Trace("One", () => anInt)
from showrTwo in Showr.ForTrace(() => Please.AllowMe().ToReplace<int>(a => $"OTHER {a}"))
from traceTwo in Checkr.Trace("Two", () => anInt)
from fail in Checkr.Expect("not 7", () => anInt != 7)
select Case.Closed;
```

**Report Excerpt:**  
```text
Executed:
   - Input: AnInt = 7
   - Trace: One   = PREFIXED 7
   - Trace: Two   = OTHER 7
```
> Configuration can depend on context.  

**The Model:**  
```csharp
public class Product
{
    public Guid Id { get; init; } = Guid.Empty;
    public string Name { get; init; } = string.Empty;
    public int AmountInStock { get; set; }
}

public record OrderRequest(Guid ProductId, int Amount);

public class Warehouse(List<Product> productsForSale)
{
    public List<Product> ProductsForSale => productsForSale;

    public void PlaceOrder(OrderRequest request) =>
        productsForSale.First(a => a.Id == request.ProductId).AmountInStock -= request.Amount;
}
```

**The Checkr:**  
```csharp
from warehouse in Trackr.Stashed(() => new Warehouse(ProductsForSale()))
from showr in Showr.ForInput(() =>
    Please.AllowMe().ToReplace<OrderRequest>(a =>
        $"Ordered {a.Amount} pieces of {warehouse.ProductsForSale.Single(b => a.ProductId == b.Id).Name}."))
from request in Checkr.Input("Order request", RequestFuzzr(warehouse))
from placeOrder in Checkr.Act("Place order", () => warehouse.PlaceOrder(request))
from noNegativeStock in Checkr.Expect("No negative stock",
    () => warehouse.ProductsForSale.Select(a => a.AmountInStock).All(a => a >= 0))
select Case.Closed;
```

**Report Excerpt:**  
```text
Executed: Place order
   - Input: Order request = { ProductId: b1c88a3c-444e-4776-a5c4-c660a75e9365, Amount: 53 }
                            Ordered 53 pieces of Product One.
```
A full reference to the available options can be found [here][A_ShowrReference].  

[A_ShowrReference]: #showr-reference
#### Showr Reference
##### For Trace

**The Checkr:**  
```csharp
from showr in Showr.ForTrace(() => Please.AllowMe().ToReplace<int>(a => $"PREFIXED {a}"))
from anInt in Checkr.Input("AnInt", Fuzzr.Int())
from trace in Checkr.Trace("AnInt", () => anInt)
from fail in Checkr.Expect("not 7", () => anInt != 7)
select Case.Closed;
```

**Report Excerpt:**  
```text
Executed:
   - Input: AnInt = 7
   - Trace: AnInt = PREFIXED 7
```
##### For Input

**The Checkr:**  
```csharp
from showr in Showr.ForInput(() => Please.AllowMe().ToReplace<int>(a => $"PREFIXED {a}"))
from anInt in Checkr.Input("AnInt", Fuzzr.Int())
from trace in Checkr.Trace("AnInt", () => anInt)
from fail in Checkr.Expect("not 7", () => anInt != 7)
select Case.Closed;
```

**Report Excerpt:**  
```text
Executed:
   - Input: AnInt = 7
                    PREFIXED 7
   - Trace: AnInt = 7
```
##### For Input Without Configuration

**The Checkr:**  
```csharp
from showr in Showr.ForInput()
from anInt in Checkr.Input("AnInt", Fuzzr.Int())
from trace in Checkr.Trace("AnInt", () => anInt)
from fail in Checkr.Expect("not 7", () => anInt != 7)
select Case.Closed;
```

**Report Excerpt:**  
```text
Executed:
   - Input: AnInt = 7
                    7
   - Trace: AnInt = 7
```
##### For All

**The Checkr:**  
```csharp
from showr in Showr.ForAll(() => Please.AllowMe().ToReplace<int>(a => $"PREFIXED {a}"))
from anInt in Checkr.Input("AnInt", Fuzzr.Int())
from trace in Checkr.Trace("AnInt", () => anInt)
from fail in Checkr.Expect("not 7", () => anInt != 7)
select Case.Closed;
```

**Report Excerpt:**  
```text
Executed:
   - Input: AnInt = 7
                    PREFIXED 7
   - Trace: AnInt = PREFIXED 7
```
##### None

**The Checkr:**  
```csharp
from showr in Showr.ForAll(() => Please.AllowMe().ToReplace<int>(a => $"PREFIXED {a}"))
from showrNone in Showr.None()
from anInt in Checkr.Input("AnInt", Fuzzr.Int())
from trace in Checkr.Trace("AnInt", () => anInt)
from fail in Checkr.Expect("not 7", () => anInt != 7)
select Case.Closed;
```

**Report Excerpt:**  
```text
Executed:
   - Input: AnInt = 7
   - Trace: AnInt = 7
```
#### For Classification
##### Default

**The Checkr:**  
```csharp
from expr in Checkr.Input("Expr", Fuzzr.Constant(
    new Term { L = new Num { Value = 42 }, R = new Num { Value = 0 }, Op = Operation.Div }))
from trace in Checkr.Trace("Original", () => expr)
from eval in Checkr.Act("Evaluate", expr.Evaluate)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                A_Default.cs:58:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 2 shrinks)
 Seed:                    844205832
 ------------------------------------------------------------
  Executed: Evaluate
   - Input: Expr     = { Op: Div, R: { Value: 0 } }
   - Trace: Original = { Op: Div, L: { Value: 42 }, R: { Value: 0 } }
 ===========================================================================
  !! System.DivideByZeroException: Attempted to divide by zero.
   at QuickCheckr.Tests.Drafts.J_YouCanGoYourOwnWay.N_Showr.Reference.Term.Evaluate() in C:\Code\QuickCheckr\QuickCheckr.Tests\Investigations\F_ObjectInputPrinting\ObjectInputPrinting.cs:line 33
   at QuickCheckr.Checkr.<>c__DisplayClass10_1`1.<TryCatch>b__1() in C:\Code\QuickCheckr\QuickCheckr\CheckrAct_Helper.cs:line 13
   at QuickCheckr.UnderTheHood.TheCatcher.InTheCheckr[T](CheckrState state, String key, Func`1 func, Action`1 onException) in C:\Code\QuickCheckr\QuickCheckr\UnderTheHood\TheCatcher.cs:line 15
 ===========================================================================
```
##### Customized

**The Checkr:**  
```csharp
from showrTerm in Showr.ForClassification(a => a.For<Term>()
    .Bounds("( ", " )")
    .Delimiter(" ")
    .HidePropertyNames()
    .Property(a => a.L)
    .Property(a => a.Op, a =>
        a switch
        {
            Operation.Add => "+",
            Operation.Div => "/",
            _ => throw new ArgumentOutOfRangeException()
        })
    .Property(a => a.R))
from showrOp in Showr.ForClassification(a => a.For<Num>()
    .Bounds("", "")
    .Delimiter("")
    .HidePropertyNames())
from expr in Checkr.Input("Expr", Fuzzr.Constant(
    new Term { L = new Num { Value = 84 }, R = new Num { Value = 2 }, Op = Operation.Div }))
from trace in Checkr.Trace("Original", () => expr)
from eval in Checkr.Act("Evaluate", expr.Evaluate)
from expect in Checkr.Expect("Not 42", () => eval != 42)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                B_Customized.cs:32:1
 Original failing run:    1 execution
 Seed:                    1611568796
 ------------------------------------------------------------
  Executed: Evaluate
   - Input: Expr     = ( 84 / 2 )
   - Trace: Original = { Op: Div, L: { Value: 84 }, R: { Value: 2 } }
 =================================
  !! Expectation Failed: Not 42
 =================================
```
##### Customized Nested

**The Checkr:**  
```csharp
from showrTerm in Showr.ForClassification(a => a.For<Term>()
    .Bounds("( ", " )")
    .Delimiter(" ")
    .HidePropertyNames()
    .Property(a => a.L)
    .Property(a => a.Op, a =>
        a switch
        {
            Operation.Add => "+",
            Operation.Div => "/",
            _ => throw new ArgumentOutOfRangeException()
        })
    .Property(a => a.R))
from showrOp in Showr.ForClassification(a => a.For<Num>()
    .Bounds("", "")
    .Delimiter("")
    .HidePropertyNames())
from expr in Checkr.Input("Expr", Fuzzr.Constant(
    new Term
    {
        L = new Num { Value = 84 },
        R = new Term { L = new Num { Value = 1 }, R = new Num { Value = 1 }, Op = Operation.Add },
        Op = Operation.Div
    }))
from trace in Checkr.Trace("Original", () => expr)
from eval in Checkr.Act("Evaluate", expr.Evaluate)
from expect in Checkr.Expect("Not 42", () => eval != 42)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                C_CustomizedNested.cs:32:1
 Original failing run:    1 execution
 Seed:                    1611568796
 ------------------------------------------------------------
  Executed: Evaluate
   - Input: Expr     = ( 84 / ( 1 + 1 ) )
   - Trace: Original = { Op: Div, L: { Value: 84 }, R: { Op: Add, L: { Value: 1 }, R: { Value: 1 } } }
 =================================
  !! Expectation Failed: Not 42
 =================================
```
##### Customized With Irrelevant

**The Checkr:**  
```csharp
from showrTerm in Showr.ForClassification(a => a.For<Term>()
    .Bounds("( ", " )")
    .Delimiter(" ")
    .HidePropertyNames()
    .Property(a => a.L)
    .Property(a => a.Op, a =>
        a switch
        {
            Operation.Add => "+",
            Operation.Div => "/",
            _ => throw new ArgumentOutOfRangeException()
        })
    .Property(a => a.R))
from showrOp in Showr.ForClassification(a => a.For<Num>()
    .Bounds("", "")
    .Delimiter("")
    .HidePropertyNames())
from expr in Checkr.Input("Expr", Fuzzr.Constant(
    new Term { L = new Num { Value = 84 }, R = new Num { Value = 0 }, Op = Operation.Div }))
from trace in Checkr.Trace("Original", () => expr)
from eval in Checkr.Act("Evaluate", expr.Evaluate)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                D_CustomizedWithIrrelevant.cs:32:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 2 shrinks)
 Seed:                    1611568796
 ------------------------------------------------------------
  Executed: Evaluate
   - Input: Expr     = ( _ / 0 )
   - Trace: Original = { Op: Div, L: { Value: 84 }, R: { Value: 0 } }
 ===========================================================================
  !! System.DivideByZeroException: Attempted to divide by zero.
   at QuickCheckr.Tests.Drafts.J_YouCanGoYourOwnWay.N_Showr.Reference.Term.Evaluate() in C:\Code\QuickCheckr\QuickCheckr.Tests\Investigations\F_ObjectInputPrinting\ObjectInputPrinting.cs:line 33
   at QuickCheckr.Checkr.<>c__DisplayClass10_1`1.<TryCatch>b__1() in C:\Code\QuickCheckr\QuickCheckr\CheckrAct_Helper.cs:line 13
   at QuickCheckr.UnderTheHood.TheCatcher.InTheCheckr[T](CheckrState state, String key, Func`1 func, Action`1 onException) in C:\Code\QuickCheckr\QuickCheckr\UnderTheHood\TheCatcher.cs:line 15
 ===========================================================================
```
##### Customized With Reducer

**The Checkr:**  
```csharp
from showrTerm in Showr.ForClassification(a => a.For<Term>()
    .Bounds("( ", " )")
    .Delimiter(" ")
    .HidePropertyNames()
    .Property(a => a.L)
    .Property(a => a.Op, a =>
        a switch
        {
            Operation.Add => "+",
            Operation.Div => "/",
            _ => throw new ArgumentOutOfRangeException()
        })
    .Property(a => a.R))
from showrOp in Showr.ForClassification(a => a.For<Num>()
    .Bounds("", "")
    .Delimiter("")
    .HidePropertyNames())
from expr in Checkr.Input("Expr", Fuzzr.Constant(
    new Term
    {
        L = new Num { Value = 84 },
        R = new Term { L = new Num { Value = 10 }, R = new Num { Value = 20 }, Op = Operation.Div },
        Op = Operation.Div
    }),
    Shrink.OnType<int>(s => s.Simplify(Reduce.Towards(1))))
from trace in Checkr.Trace("Original", () => expr)
from eval in Checkr.Act("Evaluate", expr.Evaluate)
from expect in Checkr.Expect("Not 42", () => eval != 42)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                E_CustomizedWithReducer.cs:32:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 8 shrinks)
 Seed:                    1611568796
 ------------------------------------------------------------
  Executed: Evaluate
   - Input: Expr     = ( _ / ( 10 / 20 ) )
            Redux      ( _ / ( 1 / 2 ) )
   - Trace: Original = { Op: Div, L: { Value: 84 }, R: { Op: Div, L: { Value: 10 }, R: { Value: 20 } } }
 ===========================================================================
  !! System.DivideByZeroException: Attempted to divide by zero.
   at QuickCheckr.Tests.Drafts.J_YouCanGoYourOwnWay.N_Showr.Reference.Term.Evaluate() in C:\Code\QuickCheckr\QuickCheckr.Tests\Investigations\F_ObjectInputPrinting\ObjectInputPrinting.cs:line 33
   at QuickCheckr.Checkr.<>c__DisplayClass10_1`1.<TryCatch>b__1() in C:\Code\QuickCheckr\QuickCheckr\CheckrAct_Helper.cs:line 13
   at QuickCheckr.UnderTheHood.TheCatcher.InTheCheckr[T](CheckrState state, String key, Func`1 func, Action`1 onException) in C:\Code\QuickCheckr\QuickCheckr\UnderTheHood\TheCatcher.cs:line 15
 ===========================================================================
 Passed Expectations
 - Not 42: 1x
 ------------------------------------------------------------
```
## Honey I Shrunk the Inputs
This is a collection of simple shrinking examples organized per type.  
They serve both as an accurate description of input shrinking behaviour, and as regression tests.  

So if you file an issue regarding input shrinking, you will likely see a new example added here.  
### Primitives
By default QuickCheckr does not *mutate* primitive input values, it only marks them as relevant or irrelevant  
#### Built In Enumerations

**The Checkr:**  
```csharp
from inputOne in Checkr.Input("One", Fuzzr.Enum<DayOfWeek>())
from inputTwo in Checkr.Input("Two", Fuzzr.Enum<DayOfWeek>())
from traceOne in Checkr.Trace("Original One", () => inputOne)
from traceTwo in Checkr.Trace("Original Two", () => inputTwo)
from expect in Checkr.Expect("Two should not be Monday", () => inputTwo != DayOfWeek.Monday)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                A_BuiltInEnumerations.cs:59:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    2083214969
 ------------------------------------------------------------
  Executed:
   - Input: Two          = Monday
   - Trace: Original One = Wednesday
   - Trace: Original Two = Monday
 ===================================================
  !! Expectation Failed: Two should not be Monday
 ===================================================
```
#### Custom Enumerations

**The Checkr:**  
```csharp
from inputOne in Checkr.Input("One", Fuzzr.Enum<SoundCheck>())
from inputTwo in Checkr.Input("Two", Fuzzr.Enum<SoundCheck>())
from traceOne in Checkr.Trace("Original One", () => inputOne)
from traceTwo in Checkr.Trace("Original Two", () => inputTwo)
from expect in Checkr.Expect("Two should not be Two", () => inputTwo != SoundCheck.Two)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                B_CustomEnumerations.cs:59:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    1735186993
 ------------------------------------------------------------
  Executed:
   - Input: Two          = Two
   - Trace: Original One = Two
   - Trace: Original Two = Two
 ================================================
  !! Expectation Failed: Two should not be Two
 ================================================
```
#### Nullable Types

**The Checkr:**  
```csharp
from input in Checkr.Input("Input", Fuzzr.Constant<Nullable<int>>(null!))
from expect in Checkr.Expect("Fails", () => input.HasValue)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                C_NullableTypes.cs:32:1
 Original failing run:    1 execution
 Seed:                    393010076
 ------------------------------------------------------------
  Executed:
   - Input: Input = null
 ================================
  !! Expectation Failed: Fails
 ================================
```
### Collections
When shrinking collections, QuickCheckr does not attempt to rewrite or regenerate the list.
Instead, it removes items one by one,
replaying the run each time to test whether the removed element was relevant for the failure.

This means that if only some elements in a collection contribute to the failing condition,
QuickCheckr will aggressively trim the rest away.
The result is a minimal collection containing only the values that still trigger the failure,
even if the original list was large or noisy.
  
#### Removing Irrelevant Items
Consider a test that forbids the value `7`.  

**The Checkr:**  
```csharp
from listInput in Checkr.Input("List Input", Fuzzr.Int().Many(0, 10))
from expect in Checkr.Expect("List does not contain 7", () => !listInput.Contains(7))
from trace in Checkr.Trace("List Input", () => listInput)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                A_RemovingIrrelevantItems.cs:59:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 8 shrinks)
 Seed:                    1833095082
 ------------------------------------------------------------
  Executed:
   - Input: List Input = [ 7 ]
   - Trace: List Input = [ 69, 49, 90, 36, 16, 71, 95, 7, 84 ]
 ==================================================
  !! Expectation Failed: List does not contain 7
 ==================================================
```
Even if the original list was long and varied, QuickCheckr isolates the single relevant element:  
#### Removing Irrelevant Collection
Ofcourse a collection itself is just an input like another, so if it, as a whole,
is irrelevant for the failure, QuickCheckr will remove it.  

**The Checkr:**  
```csharp
from listInput in Checkr.Input("List Input", Fuzzr.Int().Many(0, 10))
from expect in Checkr.Expect("Always fails", () => false)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                B_RemovingIrrelevantCollection.cs:59:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    2014608033
 ------------------------------------------------------------
  Executed:
   - WARNING: Empty collection deemed irrelevant at input 'List Input'.
   - WARNING: All inputs were considered irrelevant.
 =======================================
  !! Expectation Failed: Always fails
 =======================================
```
#### Irrelevant Element But Collection Length Is Not
When shrinking collections, QuickCheckr isolates the structural aspects that contributed to the failure
and leaves irrelevant details untouched. 

In cases like this example where the predicate depends solely on the length of the collection,
the individual element values play no role.  
QuickCheckr therefore preserves them as anonymous placeholders (_) and concentrates entirely on reducing the collection's shape:
removing elements, and converging toward the smallest list that still violates the condition.

This yields minimal, focused shrinking artifacts:
the report shows only the structural signal that mattered (here: the list being *too long*),
without introducing noise from element-level changes that could not affect the outcome anyway.  

**The Checkr:**  
```csharp
from list in Checkr.Input("List", Fuzzr.Int().Many(1, 10))
from trace in Checkr.Trace("Original List", () => list)
from expect in Checkr.Expect("Length <= 2", () => list.Count() <= 2)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                C_IrrelevantElementButCollectionLengthIsNot.cs:59:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 5 shrinks)
 Seed:                    1393576429
 ------------------------------------------------------------
  Executed:
   - Input: List          = [ _, _, _ ]
   - Trace: Original List = [ 12, 45, 56, 18, 31 ]
 ======================================
  !! Expectation Failed: Length <= 2
 ======================================
```
#### Multiple Relevant Elements

**The Checkr:**  
```csharp
from input in Checkr.Input("List", Fuzzr.Int(1, 10).Many(1, 10))
from list in Checkr.Capture(() => input.ToList())
from trace in Checkr.Trace("Original", () => list)
from expect in Checkr.ExpectWhen("One, Two and Three", () => input.Count() > 2, () =>
     !(list[0] == 1
    && list[1] == 2
    && list[2] == 3))
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                F_MultipleRelevantElements.cs:59:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 3 shrinks)
 Seed:                    500957855
 ------------------------------------------------------------
  Executed:
   - Input: List     = [ 1, 2, 3 ]
   - Trace: Original = [ 1, 2, 3, 9, 3, 1 ]
 =============================================
  !! Expectation Failed: One, Two and Three
 =============================================
```
#### Relevant Element And Size

**The Checkr:**  
```csharp
from input in Checkr.Input("List", Fuzzr.Int(1, 10).Many(1, 10))
from trace in Checkr.Trace("Original", () => input)
from expect in Checkr.ExpectWhen("Second element is not 7",
    () => input.Count() > 1, () => input.ElementAt(1) != 7)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                G_RelevantElementAndSize.cs:59:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 5 shrinks)
 Seed:                    1374380958
 ------------------------------------------------------------
  Executed:
   - Input: List     = [ _, 7 ]
   - Trace: Original = [ 6, 7, 9, 7, 3, 2 ]
 ==================================================
  !! Expectation Failed: Second element is not 7
 ==================================================
```
#### Nested Collections

**The Checkr:**  
```csharp
from input in Checkr.Input("List", Fuzzr.Int().Many(0, 3).Many(0, 3))
from trace in Checkr.Trace("Original", () => input)
from expect in Checkr.ExpectWhen("List[0][0] is not 42",
    () => input.Any() && input.First().Any(),
    () => input.First().First() != 42)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                H_NestedCollections.cs:59:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 4 shrinks)
 Seed:                    146712596
 ------------------------------------------------------------
  Executed:
   - Input: List     = [ [ 42 ] ]
   - Trace: Original = [ [ 42, 1, 15 ], [ 17 ], [ 27, 9 ] ]
 ===============================================
  !! Expectation Failed: List[0][0] is not 42
 ===============================================
```
#### Nested Collections Inner Count

**The Checkr:**  
```csharp
from input in Checkr.Input("List", Fuzzr.Int().Many(1, 5).Many(3, 10))
from trace in Checkr.Trace("Original", () => input)
from expect in Checkr.Expect("List count <= 4", () => input.Count() <= 4)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                H1_NestedCollectionsInnerCount.cs:32:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 5 shrinks)
 Seed:                    1860909202
 ------------------------------------------------------------
  Executed:
   - Input: List     = [ _, _, _, _, _ ]
   - Trace: Original = [ [ 34, 19 ], [ 62, 47, 14, 22, 13 ], [ 64 ], [ 42, 5, 78, 63, 94 ], [ 4, 7, 91, 94 ] ]
 ==========================================
  !! Expectation Failed: List count <= 4
 ==========================================
```
#### When An Empty Collection Is The Culprit

**The Checkr:**  
```csharp
from list in Checkr.Input("List", Fuzzr.Int().Many(0, 10))
from trace in Checkr.Trace("Original", () => list)
from listNotEmpty in Checkr.Expect("List is not empty", () => list.Any())
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                I_WhenAnEmptyCollectionIsTheCulprit.cs:32:1
 Original failing run:    1 execution
 Seed:                    2077876521
 ------------------------------------------------------------
  Executed:
   - Input: List     = [ ]
   - Trace: Original = [ ]
 ============================================
  !! Expectation Failed: List is not empty
 ============================================
```
#### The Case Of The Invisible Collection
##### The Issue
Another case of '*Greedy Shrinker Disease*'.  

This one is caused by the fact that the shrinker only sees the inputs.
It is unaware of any *state derived* meaningfull values.   

**The Model:**  
```csharp
public class Thing
{
    public List<int> Ints = [];
}
```

**The Checkr:**  
```csharp
from thing in Trackr.Stashed(() => new Thing())
from _ in Checkr.OneOfWhen(
    Checkr.Option(() => !thing.Ints.Contains(1),
        from anInt in Checkr.Input("Int", Fuzzr.Int(1, 10))
        from act in Checkr.Act("Add", () => thing.Ints.Add(anInt))
        select Case.Closed),
    Checkr.Option(() => thing.Ints.Contains(1),
        from ints in Checkr.Input("Ints", Fuzzr.Int().Many(2))
        from act in Checkr.Act("Update", () => thing.Ints = [.. ints])
        from check in Checkr.Expect("Has 1", () => thing.Ints.Contains(1))
        select Case.Closed))
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                J1_TheCaseOfTheInvisibleCollection.cs:50:1
 Original failing run:    2 executions
 Minimal failing case:    2 executions (after 1 shrink)
 Seed:                    169301180
 ------------------------------------------------------------
  Executed: Add
   - Input: Int = 1
 ------------------------------------------------------------
  Executed: Update
   - WARNING: All inputs were considered irrelevant.
 ================================
  !! Expectation Failed: Has 1
 ================================
```
##### The Solution
Supply said *state derived* meaningfull values, through a combination of a custom Shrinker and a Reducer.  

**The Checkr:**  
```csharp
from thing in Trackr.Stashed(() => new Thing())
from _ in Checkr.OneOfWhen(
    Checkr.Option(() => !thing.Ints.Contains(1),
        from anInt in Checkr.Input("Int", Fuzzr.Int(1, 10))
        from act in Checkr.Act("Add", () => thing.Ints.Add(anInt))
        from check in Checkr.Expect("Has 1", () => NewMethod(thing))
        select Case.Closed),
    Checkr.Option(() => thing.Ints.Contains(1),
        from ints in Checkr.Input("Ints",
        Fuzzr.Int().Many(2),
        Shrink.OnType<List<int>>(
            s => s.Classify(Try.Default<List<int>>(), Try.Values(thing.Ints)),
            s => s.Simplify(Reduce.Values([[], thing.Ints]))))
        from act in Checkr.Act("Update", () => thing.Ints = [.. ints])
        from trace in Checkr.Trace("Original", () => thing.Ints)
        from check in Checkr.Expect("Has 1", () => NewMethod(thing))
        select Case.Closed))
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                J2_TheCaseOfTheInvisibleCollection.cs:50:1
 Original failing run:    2 executions
 Minimal failing case:    2 executions (after 1 shrink)
 Seed:                    169301180
 ------------------------------------------------------------
  Executed: Add
   - Input: Int = 1
 ------------------------------------------------------------
  Executed: Update
   - Input: Ints     = [ 57, 70 ]
            Redux      [ ]
   - Trace: Original = [ 57, 70 ]
   - WARNING: Unused shrinkers found for 'List`1,List`1' at key 'Ints'.
 ================================
  !! Expectation Failed: Has 1
 ================================
 Passed Expectations
 - Has 1: 1x
 ------------------------------------------------------------
```
##### The Better Solution
Use the built-in `Shrink.CollectionFromState` shrinker, 
which is a convenience function that hides the plumbing required to define this type of shrinker.  

**The Checkr:**  
```csharp
from thing in Trackr.Stashed(() => new Thing())
from _ in Checkr.OneOfWhen(
    Checkr.Option(() => !thing.Ints.Contains(1),
        from anInt in Checkr.Input("Int", Fuzzr.Int(1, 10))
        from act in Checkr.Act("Add", () => thing.Ints.Add(anInt))
        select Case.Closed),
    Checkr.Option(() => thing.Ints.Contains(1),
        from ints in Checkr.Input("Ints",
        Fuzzr.Int().Many(2),
        Shrink.CollectionFromState(thing.Ints, [])) // <= Built-in Utility Shrinker
        from act in Checkr.Act("Update", () => thing.Ints = [.. ints])
        from check in Checkr.Expect("Has 1", () => thing.Ints.Contains(1))
        select Case.Closed))
select Case.Closed;
```
#### A Collection Of Objects

**The Model:**  
```csharp
public class Thing
{
    public int One { get; set; }
    public int Two { get; set; }
}
```

**The Checkr:**  
```csharp
from list in Checkr.Input("List", Fuzzr.One<Thing>().Many(2, 5))
from show in Showr.ForTrace(ListShowr)
from trace in Checkr.Trace("Original", () => list)
from expect in Checkr.ExpectWhen("Only answers for One",
    () => list.Any(), () => list.All(a => a.One != 42))
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                K_ACollectionOfObjects.cs:59:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 5 shrinks)
 Seed:                    849984174
 ------------------------------------------------------------
  Executed:
   - Input: List     = [ { One: 42 } ]
   - Trace: Original = [
    { One: 13, Two: 49 },
    { One: 61, Two: 79 },
    { One: 93, Two: 91 },
    { One: 18, Two: 39 },
    { One: 42, Two: 54 }
]
 ===============================================
  !! Expectation Failed: Only answers for One
 ===============================================
```
#### Collections Need To Grow
*Top level collections reuse the fuzzr from Input to make this work.  
For nested collections, more work/thinking is needed.*  

**The Checkr:**  
```csharp
from input in Checkr.Input("List", Fuzzr.Int().Many(3, 10))
from trace in Checkr.Trace("Original", () => input)
from expect in Checkr.Expect("List count >= 4", () => input.Count() >= 4)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                N_CollectionsNeedToGrow.cs:32:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 3 shrinks)
 Seed:                    482835926
 ------------------------------------------------------------
  Executed:
   - Input: List     = [ ]
   - Trace: Original = [ 74, 41, 59 ]
 ==========================================
  !! Expectation Failed: List count >= 4
 ==========================================
```
#### Nested Collections Need To Grow
Nested collections can in rare circumstances be classified as AssumedIrrelevant, but they might not be.  

**The Checkr:**  
```csharp
from input in Checkr.Input("Museum",
    from a in Fuzzr.Int().Many(3, 10)
    select new Museum(a))
from trace in Checkr.Trace("Original", () => input)
from expect in Checkr.Expect("List count >= 4", () => input.A.Count() >= 4)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                N1_NestedCollectionsNeedToGrow.cs:32:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 2 shrinks)
 Seed:                    482835926
 ------------------------------------------------------------
  Executed:
   - Trace: Original = { A: [ 74, 41, 59 ] }
   - WARNING: All inputs were considered irrelevant.
 ==========================================
  !! Expectation Failed: List count >= 4
 ==========================================
```
#### Nested Collections Need To Grow
Adding a Fuzzr based classification is a quick way to verify that the collection was indeed irrelevant.

Adding a reducer as well, just to check whether the empty list also fails.  

**The Checkr:**  
```csharp
from fuzzr in Checkr.Capture(() => Fuzzr.Int().Many(3, 10))
from input in Checkr.Input("Museum",
    from a in fuzzr
    select new Museum(a),
    Shrink.OnType<IEnumerable<int>>(
        s => s.Classify(
            Try.Default<IEnumerable<int>>(),
            Try.Fuzzr(fuzzr)),
        s => s.Simplify(Reduce.Values<IEnumerable<int>>([[]]))))
from trace in Checkr.Trace("Original", () => input)
from expect in Checkr.Expect("List count >= 4", () => input.A.Count() >= 4)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                N2_NestedCollectionsNeedToGrow.cs:32:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 2 shrinks)
 Seed:                    482835926
 ------------------------------------------------------------
  Executed:
   - Input: Museum   = { A: [ 74, 41, 59 ] }
            Redux      { A: [ ] }
   - Trace: Original = { A: [ 74, 41, 59 ] }
   - WARNING: No witness for reducer at 'Museum.A'.
 ==========================================
  !! Expectation Failed: List count >= 4
 ==========================================
```
#### Collection Is Null

**The Checkr:**  
```csharp
from list in Checkr.Input("List", Fuzzr.Constant<IEnumerable<int>>(null!))
from expect in Checkr.Expect("Always fails.", () => false)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                P_CollectionIsNull.cs:32:1
 Original failing run:    1 execution
 Seed:                    849984174
 ------------------------------------------------------------
  Executed:
   - Input: List = null
   - WARNING: Null value for type 'IEnumerable`1' at input 'List'.
 ========================================
  !! Expectation Failed: Always fails.
 ========================================
```
### Objects
#### Checking Nulls
**The Model:**  
```csharp
public class TheUniverse
{
    public class Life { public int Answer { get; set; } }
    public Life? Everything { get; set; }
    public Life? Irrelevant { get; set; }
    public int GetTheAnswer() => Everything!.Answer;
}
```
**The Checkr:**  
```csharp
from universe in Checkr.Input("The Universe", TheUniverseFuzzr)
from trace in Checkr.Trace("The Universe", () => universe)
from theAnswer in Checkr.Act("The Answer", () => universe.GetTheAnswer())
select Case.Closed;
```
**The Report:**  
```text
------------------------------------------------------------
 Test:                    RunTest
 Location:                Docs\M_HoneyIShrunkTheInputs\C_Objects\TheExamples\E_CheckingNulls.cs:36:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 2 shrinks)
 Seed:                    358877731
 ------------------------------------------------------------
  Executed: The Answer
   - Input: The Universe = { Everything: null }
   - Trace: The Universe = { Everything: null, Irrelevant: { Answer: 78 } }
   - WARNING: Null value for type 'Life' at input 'The Universe.Everything'.
 ===========================================================================
  !! NullReferenceException: Object reference not set to an instance of an object.
 ===========================================================================
```
Note the shrinking of the `Irrelevant` property.  
In addition, QuickCheckr reports a shrinking warning:  
> Null value for type 'Life' at input 'The Universe.Everything'.  
This tells you that the shrinker reached a `null` value that it cannot safely reduce any further.  
  
But when both `Everything` *and* `Irrelevant` are `null`, QuickCheckr cannot shrink either property, and both limitations are reported as warnings:  
```text
------------------------------------------------------------
 Test:                    RunTest_NotShrinkingNull
 Location:                Docs\M_HoneyIShrunkTheInputs\C_Objects\TheExamples\E_CheckingNulls.cs:85:1
 Original failing run:    1 execution
 Seed:                    1269557792
 ------------------------------------------------------------
  Executed: The Answer
   - Input: The Universe = { Everything: null, Irrelevant: null }
   - Trace: The Universe = { Everything: null, Irrelevant: null }
   - WARNING: Null value for type 'Life' at input 'The Universe.Everything'.
   - WARNING: Null value for type 'Life' at input 'The Universe.Irrelevant'.
 ===========================================================================
  !! NullReferenceException: Object reference not set to an instance of an object.
 ===========================================================================
```
We could help the shrinker by rewriting your checkr, in order to avoid the irrelevant input being reported as relevant.  
```csharp
from everything in Checkr.Input("Everything", TheLifeFuzzr,
    Shrink.OnType<TheUniverse.Life?>(s => s.Classify(Try.Fuzzr(TheLifeFuzzr))))
from irrelevant in Checkr.Input("Irrelevant", TheLifeFuzzr,
    Shrink.OnType<TheUniverse.Life?>(s => s.Classify(Try.Fuzzr(TheLifeFuzzr))))
from universe in Checkr.Capture(() => new TheUniverse { Everything = everything, Irrelevant = irrelevant })
from trace in Checkr.Trace("The Universe", () => universe)
from theAnswer in Checkr.Act("The Answer", () => universe.GetTheAnswer())
select Case.Closed;
```
```text
------------------------------------------------------------
 Test:                    RunTest_Alternative
 Location:                Docs\M_HoneyIShrunkTheInputs\C_Objects\TheExamples\E_CheckingNulls.cs:117:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    1269557792
 ------------------------------------------------------------
  Executed: The Answer
   - Input: Everything   = null
   - Trace: The Universe = { Everything: null, Irrelevant: null }
 ===========================================================================
  !! NullReferenceException: Object reference not set to an instance of an object.
 ===========================================================================
```
Or, we can solve the issue using a [`DeliberationPolicy`][DeliberationShrinking] that limits the number of warnings:  
```csharp
private static CheckrConfig Configure(CheckrConfig config)
    => config with { DeliberationPolicy = DeliberationPolicy.MinimumWarnings };
```
```text
------------------------------------------------------------
 Test:                    RunTest_WithDeliberationPolicy
 Location:                Docs\M_HoneyIShrunkTheInputs\C_Objects\TheExamples\E_CheckingNulls.cs:165:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 2 shrinks)
 Seed:                    1643721177
 ------------------------------------------------------------
  Executed: The Answer
   - Input: The Universe = { Everything: null }
   - Trace: The Universe = { Everything: null, Irrelevant: { Answer: 19 } }
   - WARNING: Null value for type 'Life' at input 'The Universe.Everything'.
 ===========================================================================
  !! NullReferenceException: Object reference not set to an instance of an object.
 ===========================================================================
```
As you can see the warning is still there for `Universe.Everything` but this is a relevant input,
so the warning can be ignored.  
Deliberation did succeed in removing the `Universe.Irrelevant` input from the report.  

[DeliberationShrinking]: #deliberation-shrinking
#### Tuples

**The Checkr:**  
```csharp
from input in Checkr.Input("Tuple",
    from a in Fuzzr.Int()
    from b in Fuzzr.Int()
    from c in Fuzzr.Int()
    select (a, b, c),
    Shrink.OnType<int>(s => s.Simplify(Reduce.TowardsZero)))
from irrelevant in Checkr.Input("Irrelevant",
    from a in Fuzzr.Int()
    from b in Fuzzr.Int()
    select (a, b))
from traceOne in Checkr.Trace("Original", () => input)
from expect in Checkr.Expect("Sum is < 42.", () => input.a + input.b < 42)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                J_Tuples.cs:32:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 4 shrinks)
 Seed:                    751254928
 ------------------------------------------------------------
  Executed:
   - Input: Tuple    = ( 52, 25, _ )
            Redux      ( 17, 25, _ )
   - Trace: Original = ( 52, 25, 23 )
 =======================================
  !! Expectation Failed: Sum is < 42.
 =======================================
```
#### Tuple Of Lists

**The Checkr:**  
```csharp
from input in Checkr.Input("Tuple",
    from a in Fuzzr.Int().Many(2)
    from b in Fuzzr.Int().Many(2)
    from c in Fuzzr.Int().Many(0)
    select (a, b, c),
    Shrink.OnType<int>(s => s.Simplify(Reduce.TowardsZero)))
from irrelevant in Checkr.Input("Irrelevant",
    from a in Fuzzr.Int()
    from b in Fuzzr.Int()
    select (a, b))
from traceOne in Checkr.Trace("Original", () => input)
from expect in Checkr.Expect("First list does not contain 42.", () => !input.a.Contains(42))
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                K_TupleOfLists.cs:32:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 3 shrinks)
 Seed:                    1892439403
 ------------------------------------------------------------
  Executed:
   - Input: Tuple    = ( [ 42 ], _, _ )
   - Trace: Original = ( [ 42, 32 ], [ 17, 69 ], [ ] )
 ==========================================================
  !! Expectation Failed: First list does not contain 42.
 ==========================================================
```
#### Tuples Again

**The Checkr:**  
```csharp
from input in Checkr.Input("Tuple",
    from a in Fuzzr.Int()
    from b in Fuzzr.Int()
    from c in Fuzzr.Int()
    select Tuple.Create(a, b, c),
    Shrink.OnType<int>(s => s.Simplify(Reduce.TowardsZero)))
from irrelevant in Checkr.Input("Irrelevant",
    from a in Fuzzr.Int()
    from b in Fuzzr.Int()
    select Tuple.Create(a, b))
from traceOne in Checkr.Trace("Original", () => input)
from expect in Checkr.Expect("Sum is < 42.", () => input.Item1 + input.Item2 < 42)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                L_TuplesAgain.cs:33:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 4 shrinks)
 Seed:                    751254928
 ------------------------------------------------------------
  Executed:
   - Input: Tuple    = ( 52, 25, _ )
            Redux      ( 17, 25, _ )
   - Trace: Original = { Item1: 52, Item2: 25, Item3: 23 }
 =======================================
  !! Expectation Failed: Sum is < 42.
 =======================================
```
#### Object Of Lists

**The Checkr:**  
```csharp
from input in Checkr.Input("Museum",
    from a in Fuzzr.Int().Many(2).ToList()
    from b in Fuzzr.Int().Many(2).ToList()
    from c in Fuzzr.Int().Many(0).ToList()
    select new Museum(a, b, c),
    Shrink.OnType<int>(s => s.Simplify(Reduce.TowardsZero)))
from irrelevant in Checkr.Input("Irrelevant",
    from a in Fuzzr.Int()
    from b in Fuzzr.Int()
    select (a, b))
from traceOne in Checkr.Trace("Original", () => input)
from expect in Checkr.Expect("First list does not contain 42.", () => !input.A.Contains(42))
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Q_ObjectOfLists.cs:58:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 6 shrinks)
 Seed:                    1622405126
 ------------------------------------------------------------
  Executed:
   - Input: Museum   = { A: [ 42 ] }
   - Trace: Original = { A: [ 42, 6 ], B: [ 53, 25 ], C: [ ] }
 ==========================================================
  !! Expectation Failed: First list does not contain 42.
 ==========================================================
```
### Challenges

Currently these challenges use a seeded state, because they are part of a regression test suite.

At the time of writing however, this was not the case.
The seeded report *is* representing the actual behaviour of QuickCheckr,
but for performance reasons we do not run deliberation policies everytime for instance.
  
#### Bound5
Given a 5-tuple of lists of 16-bit integers, we want to test the property that if each list sums to less than 256,
then the sum of all the values in the lists is less than 5 * 256.
This is false because of overflow. e.g. ([-20000], [-20000], [], [], []) is a counter-example.

The interesting thing about this example is the interdependence between separate parts of the sample data.
A single list in the tuple will never break the invariant, but you need at least two lists together.
This prevents most of trivial shrinking algorithms from getting close to a minimum example,
which would look somethink like ([-32768], [-1], [], [], []).
  

**The Checkr:**  
```csharp
from input in Checkr.Input("Bound5", TheFuzzr)
from trace in Checkr.Trace("Original", () => input)
from expect in Checkr.Expect(
    "The sum of all values is less than 5 * 256.",
    () => Sum(input) < (5 * 256))
select Case.Closed;
```

**The Configuration:**  
```csharp
config with
{
    DeliberationPolicy = caseFile =>
        caseFile.InputsNamed<(List<short>, List<short>, List<short>, List<short>, List<short>)>(
            "Bound5", a => ElementCount(a)),
    DeliberationTarget = 2
};
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Bound5.cs:32:1
 Original failing run:    1 execution
 Seed:                    1866106578
 ------------------------------------------------------------
  Executed:
   - Input: Bound5   = ( _, [ -21169 ], _, [ -13193 ], _ )
   - Trace: Original = ( [ ], [ -8290, -21169 ], [ ], [ -13193 ], [ ] )
 ======================================================================
  !! Expectation Failed: The sum of all values is less than 5 * 256.
 ======================================================================
```
#### Large Union List

Given a list of lists of arbitrary sized integers, 
we want to test the property that there are no more than four distinct integers across all the lists.
This is trivially false, and this example is an artificial one to stress test a shrinker's ability to normalise
(always produce the same output regardless of starting point).

In particular, a shrinker cannot hope to normalise this unless it is able to either split or join elements of the larger list.
For example, it would have to be able to transform one of [[0, 1, -1, 2, -2]] and [[0], [1], [-1], [2], [-2]] into the other.
  

**The Checkr:**  
```csharp
from input in Checkr.Input("LargeUnionList", TheFuzzr,
        Shrink.OnType<int>(s => s.Simplify(Reduce.TowardsZero)))
from trace in Checkr.Trace("Original", () => input)
from expect in Checkr.Expect(
    "No more than four distinct integers.",
    () => input.SelectMany(a => a).Distinct().Count() <= 4)
select Case.Closed;
```

**The Configuration:**  
```csharp
config with
{
    DeliberationPolicy = caseFile =>
        caseFile.InputsNamed<List<List<int>>>("LargeUnionList", a => 0 - a.Count),
    DeliberationTarget = -5
};
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                LargeUnionList.cs:32:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 25 shrinks)
 Seed:                    1284201255
 ------------------------------------------------------------
  Executed:
   - Input: LargeUnionList = [ [ _ ], [ 98 ], [ 33 ], [ 41 ], [ 73 ] ]
            Redux            [ [ _ ], [ 0 ], [ 1 ], [ 2 ], [ 3 ] ]
   - Trace: Original       = [ [ 37, 88, 82, 64, 26, 15, 91, 65 ], [ 76, 17, 18, 41, 82, 29, 89, 80, 24 ], [ 74, 39, 69, 94, 76, 44, 14, 66 ], [ 32, 98 ], [ 16, 25, 11, 59, 67, 74, 9 ], [ 63, 55, 4 ], [ ], [ 96 ], [ 16, 42, 86 ], [ 92, 57, 74, 11, 38, 67 ], [ 7, 21, 13, 49, 72, 34, 88, 10, 1, 92 ], [ 24, 61, 16 ], [ 96, 12, 20, 36, 67, 80, 80 ], [ 98 ], [ 33, 73 ], [ ], [ 41 ], [ 73 ] ]
 ===============================================================
  !! Expectation Failed: No more than four distinct integers.
 ===============================================================
```
#### Reverse List

This tests the (wrong) property that reversing a list of integers results in the same list. 
It is a basic example to validate that a library can reliably normalize simple sample data.
  

**The Checkr:**  
```csharp
from input in Checkr.Input("ReverseList", TheFuzzr)
from trace in Checkr.Trace("Original", () => input)
from reverse in Checkr.Act("Reverse List",
    () => { var list = new List<int>(input); list.Reverse(); return list; })
from expect in Checkr.Expect(
    "Reversing a list of integers results in the same list",
    () => reverse.SequenceEqual(input))
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                ReverseList.cs:32:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 4 shrinks)
 Seed:                    767678629
 ------------------------------------------------------------
  Executed: Reverse List
   - Input: ReverseList = [ _, 92 ]
   - Trace: Original    = [ 68, 89, 98, 88, 92 ]
 ================================================================================
  !! Expectation Failed: Reversing a list of integers results in the same list
 ================================================================================
```
#### Calculator

The challenge involves a simple calculator language representing expressions consisting of integers,
their additions and divisions only, like 1 + (2 / 3).

The property being tested is that

- if we have no subterms of the form x / 0,  
- then we can evaluate the expression without a zero division error.
This property is false, because we might have a term like 1 / (3 + -3), in which the divisor is not literally 0 but evaluates to 0.

One of the possible difficulties that might come up is the shrinking of recursive expressions.
  

**The Fuzzr:**  
```csharp
from i in Configr.Primitive(Fuzzr.Int(0, 10))
from noZeroDiv in Configr<Expr>.Apply(
    a =>
    {
        // rewrite division by zero terms
        if (a is Term t)
            if (t.Op == Operation.Div && t.R is Num { Value: 0 })
                t.R = new Num { Value = 1 };
    })
from depth in Configr<Expr>.Depth(0, 10)
from inheritance in Configr<Expr>.AsOneOf(typeof(Term), typeof(Num))
from terminator in Configr<Expr>.EndOn<Num>()
from expr in Fuzzr.One<Expr>()
select expr
```

**The Checkr:**  
```csharp
from input in Checkr.Input("Calculator", TheFuzzr)
from trace in Checkr.Trace("Original", () => input)
from eval in Checkr.Act("Evaluate", input.Evaluate)
select Case.Closed;
```

**The Configuration:**  
```csharp
config with { ReportMode = config.ReportMode & ~ReportMode.StackTrace };
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                A_Calculator.cs:58:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 2 shrinks)
 Seed:                    1264796639
 ------------------------------------------------------------
  Executed: Evaluate
   - Input: Calculator = { Op: Div, R: { Op: Div, L: { Value: 1 }, R: { Value: 7 } } }
   - Trace: Original   = { Op: Div, L: { Value: 4 }, R: { Op: Div, L: { Value: 1 }, R: { Value: 7 } } }
 ===========================================================================
  !! DivideByZeroException: Attempted to divide by zero.
 ===========================================================================
```
#### Calculator Adding A Reducer
And a DeliberationPolicy.  

**The Checkr:**  
```csharp
from showrTerm in Showr.ForClassification(a => a.For<Term>()
    .Bounds("( ", " )")
    .Delimiter(" ")
    .HidePropertyNames()
    .Property(a => a.L)
    .Property(a => a.Op, a =>
        a switch
        {
            Operation.Add => "+",
            Operation.Div => "/",
            _ => throw new ArgumentOutOfRangeException()
        })
    .Property(a => a.R))
from showrOp in Showr.ForClassification(a => a.For<Num>()
    .Bounds("", "")
    .Delimiter("")
    .HidePropertyNames())
from input in Checkr.Input("Calculator", TheFuzzr,
    Shrink.When<int>(s => s
            .As<Num>(a => a.Value)
            .As<Term, Expr>(a => a.R)
            .Value(t => t.Op == Operation.Div),
        s => s.Simplify(Reduce.Towards(1))),
    Shrink.OnType<int>(s => s.Simplify(Reduce.Towards(0))))
from trace in Checkr.Trace("Original", () => input)
from eval in Checkr.Act("Evaluate", input.Evaluate)
select Case.Closed;
```

**The Configuration:**  
```csharp
config with
{
    DeliberationPolicy = caseFile =>
        (caseFile.InputCount == 0 ? 100 : 0) +
        caseFile.InputsNamed<Expr>("Calculator", a => DepthOf(a)),
    DeliberationTarget = 2,
    ReportMode = config.ReportMode & ~ReportMode.StackTrace
};
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                B_CalculatorAddingAReducer.cs:32:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 12 shrinks)
 Seed:                    376086434
 ------------------------------------------------------------
  Executed: Evaluate
   - Input: Calculator = ( _ / ( 8 / _ ) )
            Redux        ( _ / ( 0 / _ ) )
   - Trace: Original   = { Op: Div, L: { Op: Add, L: { Value: 4 }, R: { Value: 6 } }, R: { Op: Div, L: { Value: 8 }, R: { Value: 9 } } }
 ===========================================================================
  !! DivideByZeroException: Attempted to divide by zero.
 ===========================================================================
```
#### Length List

A list should be generated first by picking a length between 1 and 100,
then by generating a list of precisely that length whose elements are integers between 0 and 1000.
The test should fail if the maximum value of the list is 900 or larger.

This list should specifically be generated using monadic combinators (bind) or some equivalent,
and this is a test that is only interesting for integrated shrinking.
This is only interesting as a test of a problem
[some property-based testing libraries have with monadic bind](https://clojure.github.io/test.check/growth-and-shrinking.html#unnecessary-bind).
In particular the use of the length parameter is critical,
and the challenge is to shrink this example to `[900]` reliably when using a PBT library's built in generator for lists.
  

**The Checkr:**  
```csharp
from input in Checkr.Input("Length List", TheFuzzr,
    Shrink.OnType<int>(s => s.Simplify(Reduce.TowardsZero)))
from trace in Checkr.Trace("Original", () => input)
from expect in Checkr.Expect(
    "The maximum value of the list is smaller than 900.",
    () => input.Max() < 900)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                LengthList.cs:58:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 7 shrinks)
 Seed:                    1834085712
 ------------------------------------------------------------
  Executed:
   - Input: Length List = [ 962 ]
            Redux         [ 900 ]
   - Trace: Original    = [ 974, 962, 872, 681, 543, 118, 578 ]
 ============================================================================
  !! Expectation Failed: Maximum value of the list is at smaller than 900.
 ============================================================================
```
#### Difference
##### Test1

Test 1 ('difference must not be zero') only succeeds if 
- the first parameter is less than 10 
- _or_ the difference is not zero.
The smallest falsified sample is `[10, 10]`
  

**The Checkr:**  
```csharp
from input in Checkr.Input("Input", TheFuzzr,
    Shrink.OnType<Pair>(s => s.Simplify(Reduce.Function<Pair>(a => TowardsZero(a)))))
from trace in Checkr.Trace("Original", () => input)
from expect in Checkr.Expect(
    "Difference must not be zero.",
    () => input.A < 10 || input.A != input.B)
select Case.Closed;
```

**The Reducer:**  
```csharp
public static IEnumerable<Pair> TowardsZero(Pair input)
{
    var local = input;
    while (local.A >= 0)
    {
        local = local with { A = local.A - 1, B = local.B - 1 };
        yield return local;
    }
}
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Test1.cs:32:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    1040200141
 ------------------------------------------------------------
  Executed:
   - Input: Input    = { A: 21, B: 21 }
            Redux      { A: 10, B: 10 }
   - Trace: Original = { A: 21, B: 21 }
 =======================================================
  !! Expectation Failed: difference must not be zero.
 =======================================================
```
##### Test2

Test 2 ('difference must not be small') only succeeds if 
- the first parameter is less than 10 
- _or_ the difference is not between 1 and 4.
The smallest falsified sample is `[10, 6]`.
  

**The Checkr:**  
```csharp
from input in Checkr.Input("Input", TheFuzzr,
    Shrink.OnType<Pair>(s => s.Simplify(
        Reduce.Function<Pair>(a => TowardsZero(a)))))
from trace in Checkr.Trace("Original", () => input)
from expect in Checkr.Expect(
    "Difference must not be small.",
    () => input.A < 10 || Math.Abs(input.A - input.B) > 4 || input.A == input.B)
select Case.Closed;
```

**The Reducer:**  
```csharp
public static IEnumerable<Pair> TowardsZero(Pair input)
{
    var local = input;
    while (local.A >= 0)
    {
        local = local with { A = local.A - 1, B = local.B - 1 };
        yield return local;
    }
}
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Test2.cs:32:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    737468544
 ------------------------------------------------------------
  Executed:
   - Input: Input    = { A: 53, B: 51 }
            Redux      { A: 10, B: 8 }
   - Trace: Original = { A: 53, B: 51 }
 ========================================================
  !! Expectation Failed: Difference must not be small.
 ========================================================
```
##### Test2 Better

This uses a custom reducer to shrink the input.  
It currently uses the full reducer algorithm.  
Working on turning this into a much easier way to define the reducer.  
But chaining reducers is still a work in progress.  
  

**The Checkr:**  
```csharp
from input in Checkr.Input("Input", TheFuzzr,
    Shrink.OnType<Pair>(s => s.Simplify(PairReducer.TowardsZero)))
from trace in Checkr.Trace("Original", () => input)
from expect in Checkr.Expect(
    "Difference must not be small.",
    () => input.A < 10 || Math.Abs(input.A - input.B) > 4 || input.A == input.B)
select Case.Closed;
```

**The Reducer:**  
```csharp
private class PairReducer
{
    public static Func<Reducer<Pair>> TowardsZero = () => new PairReducer().Reduce;
    private enum Phase { Both, A, B }
    private Phase phase = Phase.Both;
    public Reduction Reduce(ReductionResult result)
    {
        var current = result.ValueAs<Pair>();
        if (result.Passed)
        {
            if (phase == Phase.Both)
                phase = Phase.A;
            else if (phase == Phase.A)
                phase = Phase.B;
            if (phase == Phase.B)
                return new Reduction.Stop(current);
        }
        switch (phase)
        {
            case Phase.Both:
                if (current.A <= 0 || current.B == 0)
                    return new Reduction.Stop(current);
                return new Reduction.Next(current with { A = current.A - 1, B = current.B - 1 });
            case Phase.A:
                if (current.A <= 0)
                    return new Reduction.Stop(current);
                return new Reduction.Next(current with { B = current.B - 1 });
            case Phase.B:
                if (current.B <= 0)
                    return new Reduction.Stop(current);
                return new Reduction.Next(current with { A = current.A - 1 });
        }
        return new Reduction.Stop(current);
    }
}
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Test2Better.cs:32:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    737468544
 ------------------------------------------------------------
  Executed:
   - Input: Input    = { A: 53, B: 51 }
            Redux      { A: 10, B: 6 }
   - Trace: Original = { A: 53, B: 51 }
 ========================================================
  !! Expectation Failed: Difference must not be small.
 ========================================================
```
##### Test3

Test 3 ('difference must not be one') only succeeds if 
- the first parameter is less than 10 
- _or_ the difference is not exactly 1.
The smallest falsified sample is `[10, 9]`.
  

**The Checkr:**  
```csharp
from input in Checkr.Input("Input", TheFuzzr,
    Shrink.OnType<Pair>(s => s.Simplify(Reduce.Function<Pair>(a => TowardsZero(a)))))
from trace in Checkr.Trace("Original", () => input)
from expect in Checkr.Expect(
    "Difference is not exactly 1.",
    () => input.A < 10 || Math.Abs(input.A - input.B) != 1)
select Case.Closed;
```

**The Reducer:**  
```csharp
public static IEnumerable<Pair> TowardsZero(Pair input)
{
    var local = input;
    while (local.A >= 0)
    {
        local = local with { A = local.A - 1, B = local.B - 1 };
        yield return local;
    }
}
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Test3.cs:32:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    1238621164
 ------------------------------------------------------------
  Executed:
   - Input: Input    = { A: 49, B: 48 }
            Redux      { A: 10, B: 9 }
   - Trace: Original = { A: 49, B: 48 }
 =======================================================
  !! Expectation Failed: Difference is not exactly 1.
 =======================================================
```
This one sometimes produces 10, 11  
#### Bin Heap

This is based on an example from QuickCheck's test suite (via the SmartCheck paper). 
It generates binary heaps, and then uses a wrong implementation of a function 
that converts the binary heap to a sorted list and asserts that the result is sorted.

Interestingly most libraries seem to never find the smallest example here, 
which is the four valued heap (0, None, (0, (0, None, None), (1, None, None))). 
This is essentially because small examples are "too sparse", so it's very hard to find one by luck.
  

**The Checkr:**  
```csharp
from input in Checkr.Input("Heap", TheFuzzr)
from correct in Checkr.Act("Correct", () => Flatten(input).OrderBy(x => x).ToList())
from buggy in Checkr.Act("Buggy", () => Flatten(input).ToList())
from expect in Checkr.Expect("Heap sort is correct", () => correct.SequenceEqual(buggy))
from trace in Checkr.Trace("Original", () => input)
select Case.Closed;
```

**The Fuzzr:**  
```csharp
from depth in Configr<Heap>.Depth(3, 20)
from inheritance in Configr<Heap>.AsOneOf(typeof(Empty), typeof(Node))
from terminator in Configr<Heap>.EndOn<Empty>()
from ctor in Configr<Node>.Construct(Fuzzr.Int(), Fuzzr.One<Heap>(), Fuzzr.One<Heap>())
from value in Configr<Node>.Property(a => a.Value,
    from cnt in Fuzzr.Counter("heap") select 1000 - cnt)
from heap in Fuzzr.One<Heap>()
select heap
```

**The Configuration:**  
```csharp
config with
{
    DeliberationPolicy = caseFile =>
       (caseFile.InputCount == 0 ? 100 : 0) +
       caseFile.InputsNamed<Heap>("Heap", a => Flatten(a).Count()),
    ReportMode = config.ReportMode & ~ReportMode.Warning
};
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                BinHeap.cs:32:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    195567586
 ------------------------------------------------------------
  Executed: Buggy, Correct
   - Input: Heap     = { Left: { Value: 999, Left: { }, Right: { } }, Right: { Value: 998, Left: { }, Right: { } } }
   - Trace: Original = { Value: 997, Left: { Value: 999, Left: { }, Right: { } }, Right: { Value: 998, Left: { }, Right: { } } }
 ===============================================
  !! Expectation Failed: Heap sort is correct
 ===============================================
```
#### Coupling

In this example the elements of a list of integers are coupled to their position in an unusual way.

The expected smallest falsified sample is [1, 0].
  

**The Checkr:**  
```csharp
from length in Checkr.Input("Length", Fuzzr.Int(1, 100))
from list in Checkr.Input("List", () =>
    from enumerable in Fuzzr.Int(0, length - 1).Many(length)
    select enumerable.ToList())
from trace in Checkr.Trace("Original", () => list)
from expect in Checkr.ExpectWhen("No two different indexes point to each other.",
    () => list.All(element => element < list.Count),
    () => list.All(element =>
    {
        int index = list[element];
        if (index != element && list[index] == element)
            return false;
        return true;
    }))
select Case.Closed;
```

**The Configuration:**  
```csharp
config with
{
    DeliberationPolicy = caseFile =>
        (caseFile.InputCount == 0 ? 100 : 0) +
        caseFile.InputsNamed<List<int>>("List", a => a.Count),
    DeliberationTarget = 2,
    ReportMode = config.ReportMode & ~ReportMode.StackTrace
};
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Coupling.cs:58:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 3 shrinks)
 Seed:                    124252565
 ------------------------------------------------------------
  Executed:
   - Input: List     = [ 1, 0 ]
   - Trace: Original = [ 2, 1, 0, 1 ]
 ========================================================================
  !! Expectation Failed: No two different indexes point to each other.
 ========================================================================
```
#### Deletion

This tests the property "if we remove an element from a list, the element is
no longer in the list". The remove function we use however only actually
removes the _first_ instance of the element, so this fails whenever the list
contains a duplicate and we try to remove one of those elements.

This example is interesting for a couple of reasons:

1. It's a nice easy to explain example of property-based testing.
2. Shrinking duplicates simultaneously is something that most property-based
   testing libraries can't do.

The expected smallest falsified sample is `([0, 0], 0)`.
  

**The Checkr:**  
```csharp
from input in Checkr.Input("Deletion", Fuzzr.Int(1, 10).Many(3, 20))
from toRemove in Checkr.Input("Element To Remove", () => Fuzzr.OneOf(input))
from removed in Checkr.Act("Remove Element", () =>
{
    var list = input.ToList();
    list.Remove(toRemove);
    return list;
})
from expect in Checkr.Expect("Element is no longer in the list", () => !removed.Contains(toRemove))
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Deletion.cs:58:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 16 shrinks)
 Seed:                    613616447
 ------------------------------------------------------------
  Executed: Remove Element
   - Input: Deletion          = [ 3, 3 ]
   - Input: Element To Remove = 3
 ===========================================================
  !! Expectation Failed: Element is no longer in the list
 ===========================================================
```
#### Distinct

This tests the example provided for the property "a list of integers containing at least three distinct elements".

This is interesting because:

1. Most property-based testing libraries will not successfully normalize (i.e. always return the same answer) this property,
because it requires reordering examples to do so.
2. Hypothesis and test.check both provide a built in generator for "a list of distinct elements",
so the "example of size at least N" provides a sort of lower bound for how well they can shrink those built in generators.

The expected smallest falsified sample is `[0, 1, -1]` or `[0, 1, 2]`.
  

**The Checkr:**  
```csharp
from input in Checkr.Input("Distinct", Fuzzr.Int(1, 10).Many(3, 20),
    Shrink.OnType<int>(s => s.Simplify(Reduce.TowardsZero)))
from trace in Checkr.Trace("Original", () => input)
from expect in Checkr.ExpectWhen(
    "Contains at least three distinct elements.",
    () => input.Count() >= 3,
    () => input.ToHashSet().Count >= 3)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Distinct.cs:32:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    767903368
 ------------------------------------------------------------
  Executed:
   - Input: Distinct = [ _, 8, 8 ]
   - Trace: Original = [ 5, 8, 8 ]
 =====================================================================
  !! Expectation Failed: Contains at least three distinct elements.
 =====================================================================
```
#### Nested Lists

**The Checkr:**  
```csharp
from input in Checkr.Input("Nested List", TheFuzzr)
from expect in Checkr.Expect(
    "The sum of lengths of the element lists is at most 10",
    () => input.Sum(a => a.Count) <= 10)
select Case.Closed;
```

**The Configuration:**  
```csharp
config with
{
    DeliberationPolicy = caseFile =>
        caseFile.InputsNamed<List<List<int>>>("Nested List", a => a.Count),
};
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                NestedLists.cs:32:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 30 shrinks)
 Seed:                    1716272349
 ------------------------------------------------------------
  Executed:
   - Input: Nested List = [ [ _, _, _, _, _, _, _, _, _, _, _ ] ]
 ================================================================================
  !! Expectation Failed: The sum of lengths of the element lists is at most 10
 ================================================================================
```
## When Things Go Wrong
### What Happens When An Exception Is Thrown
- Act/Capture/Perform exception   => failure
- ActCarefully                    => inspect/assert exception behaviour
- Anything else                   => throws or rethrows
- Shrinking replay                => never throws  
#### During Act
Whenever an excecption occurs during the execution of an `Act` it is considered a failure.  

**The Checkr:**  
```csharp
from input in Checkr.Input("Counter", Fuzzr.Counter("cnt"))
from expect in Checkr.Act("Sometimes throws", () =>
{
    if (input > 0)
        ComputerSays.No("Only one is available");
})
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                A_DuringAct.cs:68:1
 Original failing run:    1 execution
 Seed:                    228179617
 ------------------------------------------------------------
  Executed: Sometimes throws
   - Input: Counter = 1
 ===========================================================================
  !! QuickPulse.Instruments.ComputerSaysNo: Only one is available
   at QuickPulse.Instruments.ComputerSays.No(String msg)
   at QuickCheckr.Tests.Drafts.P_WhenThingsGoWrong.D_WhatHappensWhenAnExceptionIsThrown.A_DuringAct.A_DuringAct.<>c__DisplayClass5_0.<TheCheckr>b__2() in C:\Code\QuickCheckr\QuickCheckr.Tests\Drafts\P_WhenThingsGoWrong\D_WhatHappensWhenAnExceptionIsThrown\A_DuringAct\A_DuringAct.cs:line 30
   at QuickCheckr.Checkr.<>c__DisplayClass2_0.<Act>b__0() in C:\Code\QuickCheckr\QuickCheckr\CheckrAct.cs:line 24
   at QuickCheckr.Checkr.<>c__DisplayClass10_1`1.<TryCatch>b__1() in C:\Code\QuickCheckr\QuickCheckr\CheckrAct_Helper.cs:line 13
   at QuickCheckr.UnderTheHood.TheCatcher.InTheCheckr[T](CheckrState state, Func`1 func, Action`1 onException) in C:\Code\QuickCheckr\QuickCheckr\UnderTheHood\TheCatcher.cs:line 12
 ===========================================================================
```
#### Checking The Exception

**The Checkr:**  
```csharp
from result in Checkr.ActCarefully("Act", () => throw new Exception("BOOM"))
from throws in Checkr.Expect("Throws", () => result.Threw)
from throwsExact in Checkr.Expect("Throws InvalidOperationException", result.ThrewAs<InvalidOperationException>)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                CheckingTheException.cs:33:1
 Original failing run:    1 execution
 Seed:                    412717959
 ------------------------------------------------------------
  Executed: Act
 ===========================================================
  !! Expectation Failed: Throws InvalidOperationException
 ===========================================================
 Passed Expectations
 - Throws: 1x
 ------------------------------------------------------------
```
#### Input Throws
Whenever an input throws, the exception is wrapped into a `InputFuzzrThrewException`.  
The Checkr below will throw a QuickFuzzr `OneOfEmptyOptionsException`:  
```csharp
from input in Checkr.Input("AnInt", Fuzzr.OneOf<int>((List<int>)[]))
select Case.Closed;
```
```csharp
Input 'AnInt' threw a 'OneOfEmptyOptionsException', see InnerException.
This might mean something is wrong with the way you wrote the Fuzzr.
But it might also mean that the fuzzr is depending on state.
And as 'Input' is eagerly evaluated, this can cause problems.
In the latter case please try using the 'lazy' version. 
Example: 'Checkr.Input(() => Fuzzr.Int()).
```
As you can gather from the exception message, this is done in order to inform the user of a rare, but real,
issue when a Fuzzr depends on external state.  
The solution is also supplied in said message:   
```csharp
Checkr.Input("AnInt", () => Fuzzr.Int());
```
If the *lazy* version throws:  
```csharp
from input in Checkr.Input("AnInt", () => Fuzzr.OneOf<int>((List<int>)[]))
select Case.Closed;
```
The Exception is **not** wrapped.  
```csharp
Fuzzr.OneOf<Int32> cannot select from an empty sequence.
Possible solutions:
- Provide at least one option (ensure the sequence is non-empty).
- Use a fallback: Fuzzr.OneOf(values).WithDefault()
- Guard upstream: values.Any() ? Fuzzr.OneOf(values) : Fuzzr.Constant(default!).
```
