# Nested Collections Inner Count

**The Checkr:**  
```csharp
from input in Checkr.Input("List", Fuzzr.Int().Many(1, 5).Many(3, 10))
from trace in Checkr.Trace("Original", () => input)
from expect in Checkr.Expect("List count <= 4", () => input.Count() <= 4)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                H1_NestedCollectionsInnerCount.cs:32:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 5 shrinks)
 Seed:                    1860909202
 ------------------------------------------------------------
  Executed:
   - Input: List     = [ _, _, _, _, _ ]
   - Trace: Original = [ [ 34, 19 ], [ 62, 47, 14, 22, 13 ], [ 64 ], [ 42, 5, 78, 63, 94 ], [ 4, 7, 91, 94 ] ]
 ==========================================
  !! Expectation Failed: List count <= 4
 ==========================================
```
