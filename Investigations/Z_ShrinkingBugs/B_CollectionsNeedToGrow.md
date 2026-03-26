# Collections Need To Grow
Not necessarily a bug, but not a helpful report either.  

**The Checkr:**  
```csharp
from input in Checkr.Input("List", Fuzzr.Int().Many(3, 10))
from trace in Checkr.Trace("Original", () => input)
from expect in Checkr.Expect("List count >= 4", () => input.Count() >= 4)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                B_CollectionsNeedToGrow.cs:58:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    482835926
 ------------------------------------------------------------
  Executed:
   - Trace: Original = [ 74, 41, 59 ]
   - WARNING: All inputs were considered irrelevant.
   - WARNING: Empty collection deemed irrelevant at input 'List'.
 ==========================================
  !! Expectation Failed: List count >= 4
 ==========================================
```
