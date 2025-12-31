# Cook Book
The QuickCheckr Cookbook is a collection of examples showing common patterns, 
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
from listLength in Fuzzr.Int(0, 10)
from enumerable in Fuzzr.Int().Many(listLength)
select enumerable.ToList();
```
**The Checkr:**  
```csharp
from showr in Showr.ForInput()
from input in Checkr.Input("Input ", ListFuzzr())
from result in Checkr.Act("Result",
    () => TheReverser.Reverses(TheReverser.Reverses(input)))
from traceResult in Checkr.Trace("Result", () => result)
from expect in Checkr.Expect(
    "Input == Result", () => input.SequenceEqual(result))
select Case.Closed;
```
**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                ListReverse.cs:34:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 8 shrinks)
 Seed:                    498116108
 ------------------------------------------------------------
  Executed: Result
   - Input  = [ _, 42 ]
              [ 62, 13, 42, 71, 81, 27, 62, 28, 85 ]
   - Result = [ 42, 62, 13, 71, 81, 27, 62, 28, 85 ]
 ==========================================
  !! Expectation Failed: Input == Result
 ==========================================
```
## Testing Aggregate Behaviour (Trackr.Verify)
**Demonstrates:**
- Accumulated-state testing
- `Trackr.Verify`
- `Trackr.StopWhenProven`

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
    from verify in Trackr.Verify("Rolls",
        ("Contains 1", () => observer.Contains(1)),
        ("Contains 2", () => observer.Contains(2)),
        ("Contains 3", () => observer.Contains(3)),
        ("Contains 4", () => observer.Contains(4)),
        ("Contains 5", () => observer.Contains(5)),
        ("Contains 6", () => observer.Contains(6)))
    select Case.Closed;
```
`Trackr.Verify` is the tool you reach for when the correctness of a test depends on the accumulated state at the end of a run,
rather than the outcome of any single execution.
In this Loaded Die example, each roll is recorded in a stashed HashSet<int>, and the *verify* is evaluated only after the final execution has completed.
This makes it ideal for tests where you want to confirm that a whole series of operations, as a group, produced the expected overall behaviour.

**Report:**  
```text
------------------------------------------------------------
 Test:                    RunTest
 Location:                CookBook\L_TheLoadedDie\TheLoadedDie.cs:71:1
 Original failing run:    100 executions
 Seed:                    1975451154
 ------------------------------------------------------------
  Executed: Roll (100 Times)
 =================================================
  !!  The Verifier Disagrees: Rolls: Contains 6
 =================================================
```
Well, you probably saw this coming, but the die is bugged. `Random.Next` is upper-bound exclusive, so six is never rolled.  
A common mistake, and the verifier pointed it out in above report.

*Note:* `Trackr.Verify`never triggers shrinking, because there is no meaningful *minimal failing case*,
either the final state satisfies all verifications or it doesn't.  
### Stop! In the Name of ... Perf?
```csharp
public class DieSixFixed
{
    public int Roll() => new Random().Next(1, 7);
}
```
Ok, we fixed the bug, so now our Checkr passes.
But what if we get lucky and we satisfy the verify after the first six rolls ?
Bit of a waste to continue running the Checkr, no ?  

Let's use `Trackr.StopWhenProven`:  
```csharp
    from die in Trackr.Stashed(() => new DieSixFixed())
    from cnt in Checkr.Perform(() => ++cell.Value)
    from observer in Trackr.Stashed(() => new HashSet<int>())
    from roll in Checkr.Act("Roll", () => observer.Add(die.Roll()))
    from proven in Trackr.StopWhenProven("All sides rolled",
        () => observer.Order().SequenceEqual([1, 2, 3, 4, 5, 6])) // <=
    from verify in Trackr.Verify("Rolls",
        ("Contains 1", () => observer.Contains(1)),
        ("Contains 2", () => observer.Contains(2)),
        ("Contains 3", () => observer.Contains(3)),
        ("Contains 4", () => observer.Contains(4)),
        ("Contains 5", () => observer.Contains(5)),
        ("Contains 6", () => observer.Contains(6)))
    select Case.Closed;
```
Whenever the predicate passed into `Trackr.StopWhenProven` returns true,
the Checkr is considered proven and it concludes immediately, ... no more executions.  
