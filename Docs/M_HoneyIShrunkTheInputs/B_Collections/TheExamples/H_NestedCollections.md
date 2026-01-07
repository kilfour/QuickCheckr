# Nested Collections

**The Checkr:**  
```csharp
from input in Checkr.Input("List", Fuzzr.Int().Many(0, 3).Many(0, 3))
from trace in Checkr.Trace("Original", () => input)
from expect in Checkr.ExpectWhen("List[0][0] is not 42",
    () => input.Any() && input.First().Any(),
    () => input.First().First() != 42)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                H_NestedLists.cs:48:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 4 shrinks)
 Seed:                    146712596
 ------------------------------------------------------------
  Executed:
   - Input: List     = [ [ 42 ] ]
   - Trace: Original = [ [ 42, 1, 15 ], [ 17 ], [ 27, 9 ] ]
 ===============================================
  !! Expectation Failed: List[0][0] is not 42
 ===============================================
```
