# Verifying List Reversal (Double-Reverse Law)
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
