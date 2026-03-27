# Test2

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
