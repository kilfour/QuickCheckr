# List Element Removal

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
 Location:                ListElementRemoval.cs:58:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 4 shrinks)
 Seed:                    1609250252
 ------------------------------------------------------------
  Executed:
   - Input: Nested List = [ [ _ ], [ _, _ ] ]
   - Trace: Original    = [ [ 24, 12 ], [ 59, 39 ] ]
 ================================================================================
  !! Expectation Failed: The sum of lengths of the element lists is at most 10
 ================================================================================
```
