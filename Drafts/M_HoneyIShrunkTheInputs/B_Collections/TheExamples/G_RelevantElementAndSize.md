# Relevant Element And Size

**The Checkr:**  
```csharp
from input in Checkr.Input("List", Fuzzr.Int(1, 10).Many(1, 10))
from trace in Checkr.Trace("Original", () => input)
from expect in Checkr.ExpectWhen("Second element is not 7",
    () => input.Count() > 1, () => input.ElementAt(1) != 7)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                G_RelevantElementAndSize.cs:59:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 5 shrinks)
 Seed:                    1374380958
 ------------------------------------------------------------
  Executed:
   - Input: List     = [ _, 7 ]
   - Trace: Original = [ 6, 7, 9, 7, 3, 2 ]
 ==================================================
  !! Expectation Failed: Second element is not 7
 ==================================================
```
