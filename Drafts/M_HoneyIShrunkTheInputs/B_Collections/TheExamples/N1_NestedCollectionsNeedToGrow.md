# Nested Collections Need To Grow
Nested collections can in rare circumstances be classified as AssumedIrrelevant, but they might not be.  

**The Checkr:**  
```csharp
from input in Checkr.Input("Museum",
    from a in Fuzzr.Int().Many(3, 10)
    select new Museum(a))
from trace in Checkr.Trace("Original", () => input)
from expect in Checkr.Expect("List count >= 4", () => input.A.Count() >= 4)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                N1_NestedCollectionsNeedToGrow.cs:32:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 2 shrinks)
 Seed:                    482835926
 ------------------------------------------------------------
  Executed:
   - Trace: Original = { A: [ 74, 41, 59 ] }
   - WARNING: All inputs were considered irrelevant.
 ==========================================
  !! Expectation Failed: List count >= 4
 ==========================================
```
