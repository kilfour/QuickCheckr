# Multiple Relevant Elements

**The Checkr:**  
```csharp
from input in Checkr.Input("List", Fuzzr.Int(1, 10).Many(1, 10))
from list in Checkr.Capture(() => input.ToList())
from trace in Checkr.Trace("Original", () => list)
from expect in Checkr.ExpectWhen("One, Two and Three", () => input.Count() > 2, () =>
     !(list[0] == 1
    && list[1] == 2
    && list[2] == 3))
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                F_MultipleRelevantElements.cs:46:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 3 shrinks)
 Seed:                    500957855
 ------------------------------------------------------------
  Executed:
   - Input: List     = [ 1, 2, 3 ]
   - Trace: Original = [ 1, 2, 3, 9, 3, 1 ]
 =============================================
  !! Expectation Failed: One, Two and Three
 =============================================
```
