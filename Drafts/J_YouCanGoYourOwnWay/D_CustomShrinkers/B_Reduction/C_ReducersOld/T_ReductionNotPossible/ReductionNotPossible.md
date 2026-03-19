# Reduction Not Possible

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
