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
 Seed:                    1395068066
 ------------------------------------------------------------
  Executed:
   - Input: Nested List = [ [ _, _, _ ], [ _, _, _, _, _, _, _, _ ] ]
   - Trace: Original    = [ [ 80, 93 ], [ ], [ 47, 2, 95, 89, 7, 60 ], [ 78, 95, 99, 87, 11, 21, 75, 60 ] ]
 ================================================================================
  !! Expectation Failed: The sum of lengths of the element lists is at most 10
 ================================================================================
```
