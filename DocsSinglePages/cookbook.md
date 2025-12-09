# Cook Book
The QuickCheckr Cookbook is a collection of short, focused examples showing common patterns, 
testing techniques, and pitfalls you can check using QuickCheckr.  
## Verifying List Reversal (Double-Reverse Law)
**Demonstrates:**
- Custom Fuzzrs
- Relevant input shrinking
- Double-application property


I guess this one is very much tradition in PBT world, and who am I to argue with tradition, ... so here we go.

**The (buggy) implementation:**  
```csharp
public class TheReverser
{
    public static List<int> Reverses(List<int> list)
        => [.. HideTheAnswer(list.AsEnumerable()).Reverse()];
    private static IEnumerable<int> HideTheAnswer(IEnumerable<int> list)
        => list.Where(a => a != 42).Concat(list.Where(a => a == 42));
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
 Location:                Docs\T_Cookbook\A_ListReverse\ListReverse.cs:46:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 8 shrinks)
 Seed:                    498116108
 ------------------------------------------------------------
  Executed: Result
   - Input: Input = [ _, 42 ]
   - Trace: Input  = [ 62, 13, 42, 71, 81, 27, 62, 28, 85 ]
   - Trace: Result = [ 42, 62, 13, 71, 81, 27, 62, 28, 85 ]
 ===================================
  !! Spec Failed: Input == Result
 ===================================
```
## Testing Aggregate Behaviour (Trackr.Assay)
**Demonstrates:**
- Accumulated-state testing
- `Trackr.Assay`
- `Trackr.ProvenWhen`

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
 Location:                Docs\T_Cookbook\L_TheLoadedDie\TheLoadedDie.cs:71:1
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

*Sidenote:* `Trackr.Assay`never triggers shrinking, because there is no meaningful *minimal failing case*.
Assays evaluate the final accumulated state, so removing any execution changes the outcome.  
### `Trackr.ProvenWhen`
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
