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
 Minimal failing case:    1 execution (after 14 shrinks)
 Seed:                    468879457
 ------------------------------------------------------------
  Executed:
   - Input: Nested List = [ [ _, _, _, _, _, _, _ ], [ _, _, _, _ ] ]
   - Trace: Original    = [ [ 20, 95, 71, 63, 97, 82, 29, 70, 19 ], [ 82, 62, 30, 77, 42, 80 ], [ 33, 63, 38, 48, 24, 42, 23, 30 ], [ 13, 66, 22, 18 ] ]
 ================================================================================
  !! Expectation Failed: The sum of lengths of the element lists is at most 10
 ================================================================================
```
