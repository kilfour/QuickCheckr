# Issue Maybe

**The Checkr:**  
```csharp
from answers in Checkr.Input("answers", Fuzzr.Int().Many(1))
from trace in Checkr.Trace("answers", () => answers)
from expect in Checkr.Expect("all answers equal 42", () => answers.All(a => a == 42))
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                IssueMaybe.cs:56:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    346495699
 ------------------------------------------------------------
  Executed:
   - Input: answers = [ _ ]
   - Trace: answers = [ 15 ]
 ===============================================
  !! Expectation Failed: all answers equal 42
 ===============================================
```
