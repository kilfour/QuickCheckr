# Nested Collections Inner Count
Adding the custom shrinker solves the problem in the report,
but really coming up with that solution is not something you want the user to do.
  

**The Checkr:**  
```csharp
from input in Checkr.Input("List", Fuzzr.Int().Many(1, 5).Many(3, 10),
    Shrink.CollectionFromState<IEnumerable<int>>([[], [], [], [], []], []))
from trace in Checkr.Trace("Original", () => input)
from expect in Checkr.Expect("List count >= 4", () => input.Count() >= 4)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                B1_NestedCollectionsInnerCount.cs:32:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    1134566081
 ------------------------------------------------------------
  Executed:
   - Input: List     = [ [ 79, 83, 60, 28, 36 ], [ 92, 99 ], [ 91 ] ]
            Redux      [ ]
   - Trace: Original = [ [ 79, 83, 60, 28, 36 ], [ 92, 99 ], [ 91 ] ]
 ==========================================
  !! Expectation Failed: List count >= 4
 ==========================================
```
