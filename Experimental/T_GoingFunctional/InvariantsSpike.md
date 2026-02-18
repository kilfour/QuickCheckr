# Invariants Spike

**The SUT:**  
```csharp
public class TheReverser
{
    public static List<int> Reverses(List<int> list)
        => [.. HideTheAnswer(list.AsEnumerable()).Reverse()];
    private static IEnumerable<int> HideTheAnswer(IEnumerable<int> list)
        => list.Where(a => a != 42).Concat(list.Where(a => a == 42));
}
```

**The Test:**  
```csharp
Check.ForAll(
    from len in Fuzzr.Int(0, 10)
    from elements in Fuzzr.Int().Many(len)
    select elements.ToList(),
    list => list.SequenceEqual(TheReverser.Reverses(TheReverser.Reverses(list))));
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    TheTest
 Location:                Experimental\T_GoingFunctional\InvariantsSpike.cs:45:1
 Original failing run:    28 executions
 Minimal failing case:    1 execution (after 31 shrinks)
 Seed:                    851126371
 ------------------------------------------------------------
  Executed:
   - Input: Input = [ _, 42 ]
 ===================================
  !! Expectation Failed: Property
 ===================================
 Passed Expectations
 - Property: 27x
 ------------------------------------------------------------
```
