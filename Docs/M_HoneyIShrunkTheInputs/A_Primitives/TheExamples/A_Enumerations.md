# Enumerations
**The Checkr:**  
```csharp
from inputOne in Checkr.Input("One", Fuzzr.Enum<DayOfWeek>())
from inputTwo in Checkr.Input("Two", Fuzzr.Enum<DayOfWeek>())
from expect in Checkr.Expect("Two should not be Monday", () => inputTwo != DayOfWeek.Monday)
select Case.Closed;
```
**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                A_Enumerations.cs:51:1
 Original failing run:    10 executions
 Minimal failing case:    1 execution (after 10 shrinks)
 Seed:                    1017208542
 ------------------------------------------------------------
  Executed:
   - Input: Two = Monday
 ===================================================
  !! Expectation Failed: Two should not be Monday
 ===================================================
 Passed Expectations
 - Two should not be Monday: 9x
 ------------------------------------------------------------
```
