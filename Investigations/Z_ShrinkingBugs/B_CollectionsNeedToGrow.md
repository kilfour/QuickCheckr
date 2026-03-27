# Collections Need To Grow
*Top level collections reuse the fuzzr from Input to make this work.  
For nested collections, more work/thinking is needed.*  

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
 Location:                B_CollectionsNeedToGrow.cs:32:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 3 shrinks)
 Seed:                    482835926
 ------------------------------------------------------------
  Executed:
   - Input: List     = [ ]
   - Trace: Original = [ 74, 41, 59 ]
 ==========================================
  !! Expectation Failed: List count >= 4
 ==========================================
```
