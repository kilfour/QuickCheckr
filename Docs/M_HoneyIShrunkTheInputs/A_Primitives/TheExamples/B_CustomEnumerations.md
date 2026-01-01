# Custom Enumerations

**The Checkr:**  
```csharp
from inputOne in Checkr.Input("One", Fuzzr.Enum<SoundCheck>())
from inputTwo in Checkr.Input("Two", Fuzzr.Enum<SoundCheck>())
from expect in Checkr.Expect("Two should not be Two", () => inputTwo != SoundCheck.Two)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                B_CustomEnumerations.cs:54:1
 Original failing run:    2 executions
 Minimal failing case:    1 execution (after 2 shrinks)
 Seed:                    1017208542
 ------------------------------------------------------------
  Executed:
   - Input: Two = Two
 ================================================
  !! Expectation Failed: Two should not be Two
 ================================================
 Passed Expectations
 - Two should not be Two: 1x
 ------------------------------------------------------------
```
