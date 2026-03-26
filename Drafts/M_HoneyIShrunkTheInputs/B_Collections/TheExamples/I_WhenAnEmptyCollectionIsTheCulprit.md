# When An Empty Collection Is The Culprit

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
 Location:                I_WhenAnEmptyCollectionIsTheCulprit.cs:32:1
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
