# Test3

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
