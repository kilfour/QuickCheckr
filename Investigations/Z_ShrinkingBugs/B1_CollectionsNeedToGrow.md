# Collections Need To Grow
Adding the custom shrinker solves the problem in the report,
but really coming up with that solution is not something you want the user to do.
  

**The Checkr:**  
```csharp
from fuzzr in Checkr.Capture(() => Fuzzr.Int().Many(3, 10))
from input in Checkr.Input("List", fuzzr,
    // Refuzzing can grow the list.
    // Supplying values *manually* also.
    Shrink.OnType<IEnumerable<int>>(s => s.Classify(Try.Fuzzr(fuzzr, 100))))
from trace in Checkr.Trace("Original", () => input)
from expect in Checkr.Expect("List count >= 4", () => input.Count() >= 4)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                B1_CollectionsNeedToGrow.cs:32:1
 Original failing run:    1 execution
 Seed:                    1134566081
 ------------------------------------------------------------
  Executed:
   - Input: List     = [ 87, 79, 83 ]
   - Trace: Original = [ 87, 79, 83 ]
 ==========================================
  !! Expectation Failed: List count >= 4
 ==========================================
```
