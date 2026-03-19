# Built In Enumerations

**The Checkr:**  
```csharp
from inputOne in Checkr.Input("One", Fuzzr.Enum<DayOfWeek>())
from inputTwo in Checkr.Input("Two", Fuzzr.Enum<DayOfWeek>())
from traceOne in Checkr.Trace("Original One", () => inputOne)
from traceTwo in Checkr.Trace("Original Two", () => inputTwo)
from expect in Checkr.Expect("Two should not be Monday", () => inputTwo != DayOfWeek.Monday)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                A_BuiltInEnumerations.cs:59:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    2083214969
 ------------------------------------------------------------
  Executed:
   - Input: Two          = Monday
   - Trace: Original One = Wednesday
   - Trace: Original Two = Monday
 ===================================================
  !! Expectation Failed: Two should not be Monday
 ===================================================
```
