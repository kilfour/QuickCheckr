# Shrinking Bug

**The Checkr:**  
```csharp
from input in Checkr.Input("Nested List", TheFuzzr)
from trace in Checkr.Trace("Original", () => input)
from expect in Checkr.Expect(
    "The sum of lengths of the element lists is at most 10",
    () => input.Sum(a => a.Count) <= 2)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                ShrinkingBug.cs:63:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 4 shrinks)
 Seed:                    2042586657
 ------------------------------------------------------------
  Executed:
   - Input: Nested List = [ [ _ ], [ _, _ ] ]
   - Trace: Original    = [ [ 77, 3 ], [ 52, 60 ] ]
 ================================================================================
  !! Expectation Failed: The sum of lengths of the element lists is at most 10
 ================================================================================
```
