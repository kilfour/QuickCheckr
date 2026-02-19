# List Containing Null Values

**The Checkr:**  
```csharp
from listInput in Checkr.Input("List Input", Fuzzr.Int().Nullable().Many(0, 10))
from expect in Checkr.Expect("no nulls allowed", () => listInput.All(a => a.HasValue))
from trace in Checkr.Trace("List Input", () => listInput)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                ListContainingNullValues.cs:64:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 8 shrinks)
 Seed:                    1833095082
 ------------------------------------------------------------
  Executed:
   - Input: List Input = [ null ]
   - Trace: List Input = [ 49, 36, null, 95, null, 82, 73, 58, 2 ]
 ===========================================
  !! Expectation Failed: no nulls allowed
 ===========================================
```
