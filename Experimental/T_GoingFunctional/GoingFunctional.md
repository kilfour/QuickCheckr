# Going Functional
## Spike

**The Model:**  
```csharp
public class TheReverser
{
    public static List<int> Reverses(List<int> list)
        => [.. FudgeIt(list.AsEnumerable()).Reverse()];
    private static IEnumerable<int> FudgeIt(IEnumerable<int> list)
        => list.Where(a => a != 6).Concat(list.Where(a => a == 6));
}
```

**The Invariant:**  
```csharp
Check.ForAll(
    from len in Fuzzr.Int(0, 10)
    from elements in Fuzzr.Int(0, 10).Many(len)
    select elements.ToList(),
    list => list.SequenceEqual(TheReverser.Reverses(TheReverser.Reverses(list))));
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                GoingFunctional.cs:27:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 7 shrinks)
 Seed:                    2116954181
 ------------------------------------------------------------
  Executed:
   - Input: Input = [ _, 6 ]
 ===================================
  !! Expectation Failed: Property
 ===================================
```
