# Reduction Not Possible
What to do if all reduction values fail ?  

**The Checkr:**  
```csharp
from input in Checkr.Input("Input", Fuzzr.Int(), Shrink.OnType(Reduce.TowardsZero))
from expect in Checkr.Expect("Input should be greater than 42", () => input > 42)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                ReductionNotPossible.cs:64:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    1373577700
 ------------------------------------------------------------
  Executed:
   - Input: Input = 21
            Redux   0
 ==========================================================
  !! Expectation Failed: Input should be greater than 42
 ==========================================================
```
