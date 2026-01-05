# When An Empty List Is The Culprit

**The Checkr:**  
```csharp
from list in Checkr.Input("List", Fuzzr.Int().Many(0, 10))
from trace in Checkr.Trace("Original", () => list)
from listNotEmpty in Checkr.Expect("List is not empty", () => list.Any())
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                I_WhenAnEmptyListIsTheCulprit.cs:48:1
 Original failing run:    1 execution
 Seed:                    2077876521
 ------------------------------------------------------------
  Executed:
   - Input: List     = [ ]
   - Trace: Original = [ ]
 ============================================
  !! Expectation Failed: List is not empty
 ============================================
```
