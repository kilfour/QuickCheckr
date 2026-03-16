# Test1

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
