## Putting a Case on the Board
### Test Framework Integration
QuickCheckr is fully **framework agnostic**: it doesn't care whether you run your tests with xUnit, NUnit, MSTest, or TUnit. There are no base classes to inherit from, no runner integrations to install, and no magic attributes controlling behaviour.  

A Checkr is just a LINQ expression that you evaluate by calling `Run()`.  

If a minimal failing case is found, QuickCheckr reports it by **throwing a `FalsifiableException`**, which contains the complete shrunk report, including the seed, execution steps, inputs, actions, traces, and the reason for failure.  

Because the failure is surfaced as a standard exception, any test framework can treat it as a normal assertion failure and you get a rich, human-readable report for free.  
### Running a Checkr
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
which allows for controlling specific aspects of the Checkr run. We'll get into that later.  
#### Filing Evidence
QuickCheckr can be configured to persist the results of a failed test run.  
```csharp
private static CheckrConfig Configure(CheckrConfig config)
    => config with { FileAs = "MyQuickCheckrTest" };
```
Above configuration will write the resulting report of a failing run to a `report.qr` file in a folder relative to the solution root:  
```csharp
.quickcheckr\MyQuickCheckrTest\report.qr
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
#### Investigations
These tests show the initial version of QuickCheckrs logging configuration.  
## Configuration
QuickCheckr runs are configured through an immutable `CheckrConfig` record. 
You typically start from `CheckrConfig.Default()` and then tweak the parts you care about  
## The Evidence Vault
Once an evidence vault exists the seeds therein contained can be reviewed.  
This is done by calling `ReviewColdCases` on the Checkr instance, passing in the same configuration used to create the evidence vault.  
```csharp
checkr.ReviewColdCases(Configure);
```
It is advised to *Dry* out the configuration as is shown at the top of this chapter, in order to avoid code drift.  
This will create a report per seed in the evidence file and a log of all evidence vault actions.
  
- For every failing seed the report will look like the by now familiar failure report.  
- For every passing seed the report will only contain a list of passed specs.  
## Cook Book
### List Reverse
I guess this one is very much tradition in PBT world, and who am I to argue with tradition.  
So here we go.

**The (buggy) implementation:**  
```csharp
public class TheReverser
{
    public static List<int> Reverses(List<int> list)
        => [.. HideTheAnswer(list.AsEnumerable()).Reverse()];
    private static IEnumerable<int> HideTheAnswer(IEnumerable<int> list)
        => list.Where(a => a != 42);
}
```
**The Fuzzr:**  
```csharp
var listFuzzr =
    from listLength in Fuzzr.Int(0, 10)
    from enumerable in Fuzzr.Int().Many(listLength)
    select enumerable.ToList();
```
**The Checkr:**  
```csharp
    from input in Checkr.Input("Input", ListFuzzr())
    from result in Checkr.Act("Result",
        () => TheReverser.Reverses(TheReverser.Reverses(input)))
    from trace in Checkr.Trace("Input ", () => input)
    from traceResult in Checkr.Trace("Result", () => result)
    from spec in Checkr.Spec(
        "Input == Result", () => input.SequenceEqual(result))
    select Case.Closed;
```
**The Report:**  
```text
------------------------------------------------------------
 Test:                    RunTest
 Location:                Drafts\T_Cookbook\A_ListReverse\ListReverse.cs:65:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 8 shrinks)
 Seed:                    498116108
 ------------------------------------------------------------
  Executed: Result
   - Input: Input = [ 42 ]
   - Trace: Input  = [ 62, 13, 42, 71, 81, 27, 62, 28, 85 ]
   - Trace: Result = [ 62, 13, 71, 81, 27, 62, 28, 85 ]
 ===================================
  !! Spec Failed: Input == Result
 ===================================
```
### The Loaded Die
While not strictly speaking a property-test, as there is no shrinking involved,
this kind of test is nevertheless very useful.  
So much so that QuickCheckr provides a dedicated tool for it. 

We're testing a simple implementation of a die:
  
```csharp
public class DieSix
{
    public int Roll() => new Random().Next(1, 6);
}
```
And we want to ensure that it rolls the numbers 1-6.  For which use the following Checkr:  
```csharp
    from die in Trackr.Stashed(() => new DieSix())
    from observer in Trackr.Stashed(() => new HashSet<int>())
    from roll in Checkr.Act("Roll", () => observer.Add(die.Roll()))
    from assay in Trackr.Assay("Rolls",
        ("Contains 1", () => observer.Contains(1)),
        ("Contains 2", () => observer.Contains(2)),
        ("Contains 3", () => observer.Contains(3)),
        ("Contains 4", () => observer.Contains(4)),
        ("Contains 5", () => observer.Contains(5)),
        ("Contains 6", () => observer.Contains(6)))
    select Case.Closed;
```
`Trackr.Assay` is the tool you reach for when the correctness of a test depends on the accumulated state at the end of a run,
rather than the outcome of any single execution.
In this Loaded Die example, each roll is recorded in a stashed HashSet<int>, and the *assay* is evaluated only after the final execution has completed.
This makes it ideal for tests where you want to confirm that a whole series of operations, as a group, produced the expected overall behaviour.

**Report:**  
```text
------------------------------------------------------------
 Test:                    RunTest
 Location:                Drafts\T_Cookbook\L_TheLoadedDie\TheLoadedDie.cs:66:1
 Original failing run:    30 executions
 Seed:                    930735070
 ------------------------------------------------------------
  Executed: Roll (30 Times)
 ================================================
  !!  The Assayer Disagrees: Rolls: Contains 6
 ================================================
```
Well, you probably saw this coming, but the die is bugged. `Random.Next` is upper-bound exclusive, so six is never rolled.  
A common mistake, and the assayer pointed it out in above report.

*Sidenote:* When one uses `Trackr.Assay` there is no shrinking whatsoever, as there is no *minimal* fail-case, by definition.  
Assays evaluate state accumulated across all executions, so removing any execution and or inputs,
changes the final state therefore shrinking is not applicable.  
#### `Trackr.ProvenWhen`
```csharp
public class DieSixFixed
{
    public int Roll() => new Random().Next(1, 7);
}
```
Ok, we fixed the bug, so now our Checkr passes.
But what if we get lucky and we satisfy the assay after the first six rolls ?
Bit of a waste to continue running the Checkr, no ?  

Let's use `Trackr.ProvenWhen`:  
```csharp
    from die in Trackr.Stashed(() => new DieSixFixed())
    from observer in Trackr.Stashed(() => new HashSet<int>())
    from roll in Checkr.Act("Roll", () => observer.Add(die.Roll()))
    from proven in Trackr.ProvenWhen("All sides rolled",
        () => observer.Order().SequenceEqual([1, 2, 3, 4, 5, 6])) // <=
    from assay in Trackr.Assay("Rolls",
        ("Contains 1", () => observer.Contains(1)),
        ("Contains 2", () => observer.Contains(2)),
        ("Contains 3", () => observer.Contains(3)),
        ("Contains 4", () => observer.Contains(4)),
        ("Contains 5", () => observer.Contains(5)),
        ("Contains 6", () => observer.Contains(6)))
    select Case.Closed;
```
Whenever the predicate passed into `Trackr.ProvenWhen` returns true,
the Checkr is considered proven and it concludes immediately, ... no more executions.  
