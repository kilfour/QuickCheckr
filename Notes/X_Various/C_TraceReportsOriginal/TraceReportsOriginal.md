# Trace Reports Original

**The Checkr:**  
```csharp
from x in Checkr.Input("x", Fuzzr.Int(-100, 100), Reduce.TowardsZero)
from trace in Checkr.Trace("x (trace)", () => x)
from expect in Checkr.Expect("x must be negative", () => x >= 0)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                TraceReportsOriginal.cs:37:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    1468295090
 ------------------------------------------------------------
  Executed:
   - Input: x         = -12
            Redux       -1
   - Trace: x (trace) = -12
 =============================================
  !! Expectation Failed: x must be negative
 =============================================
```
