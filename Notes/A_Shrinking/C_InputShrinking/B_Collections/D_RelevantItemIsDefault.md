# Relevant Item Is Default
Just making sure `default` values, `0` in this case, get handled correctly.  

**The Checkr:**  
```csharp
from listInput in Checkr.Input("List Input", Fuzzr.Int(0, 100).Many(0, 10))
from expect in Checkr.Expect("List does not contain 0", () => !listInput.Contains(0))
from trace in Checkr.Trace("List Input", () => listInput)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                D_RelevantItemIsDefault.cs:59:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 8 shrinks)
 Seed:                    820833320
 ------------------------------------------------------------
  Executed:
   - Input: List Input = [ 0 ]
   - Trace: List Input = [ 84, 63, 90, 91, 55, 53, 39, 91, 0 ]
 ==================================================
  !! Expectation Failed: List does not contain 0
 ==================================================
```
