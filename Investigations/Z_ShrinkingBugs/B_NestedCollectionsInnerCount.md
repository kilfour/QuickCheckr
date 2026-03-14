# Nested Collections Inner Count
Not necessarily a bug, but not a helpful report either. 

There is a real bug in the shrink count though.  
Only the outer list shrink (being deemed irrelevant) is counted, minor issue.
  

**The Checkr:**  
```csharp
from input in Checkr.Input("List", Fuzzr.Int().Many(1, 5).Many(3, 10))
from trace in Checkr.Trace("Original", () => input)
from expect in Checkr.Expect("List count >= 4", () => input.Count() >= 4)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                B_NestedCollectionsInnerCount.cs:32:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    1427464097
 ------------------------------------------------------------
  Executed:
   - Trace: Original = [ [ 41, 73, 3, 5, 14 ], [ 2, 88, 10 ], [ 59, 61, 21, 32, 34 ] ]
   - WARNING: All inputs were considered irrelevant.
   - WARNING: Empty collection deemed irrelevant at input 'List'.
 ==========================================
  !! Expectation Failed: List count >= 4
 ==========================================
```
