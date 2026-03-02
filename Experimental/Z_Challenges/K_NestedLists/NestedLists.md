# Nested Lists

**The Checkr:**  
```csharp
from input in Checkr.Input("Nested List", TheFuzzr)
from trace in Checkr.Trace("Original", () => input)
from expect in Checkr.Expect(
    "The sum of lengths of the element lists is at most 10",
    () => input.Sum(a => a.Count) <= 10)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                NestedLists.cs:63:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 16 shrinks)
 Seed:                    893891090
 ------------------------------------------------------------
  Executed:
   - Input: Nested List = [ [ _, _ ], [ _, _, _, _ ] ]
   - Trace: Original    = [ [ 47, 43, 25, 52, 69, 11, 17 ], [ 95, 84, 95, 19, 1, 74, 27, 26, 83 ] ]
 ================================================================================
  !! Expectation Failed: The sum of lengths of the element lists is at most 10
 ================================================================================
```
