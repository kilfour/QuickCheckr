# Custom Enumerations

**The Checkr:**  
```csharp
from inputOne in Checkr.Input("One", Fuzzr.Enum<SoundCheck>())
from inputTwo in Checkr.Input("Two", Fuzzr.Enum<SoundCheck>())
from traceOne in Checkr.Trace("Original One", () => inputOne)
from traceTwo in Checkr.Trace("Original Two", () => inputTwo)
from expect in Checkr.Expect("Two should not be Two", () => inputTwo != SoundCheck.Two)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                B_CustomEnumerations.cs:59:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    1735186993
 ------------------------------------------------------------
  Executed:
   - Input: Two          = Two
   - Trace: Original One = Two
   - Trace: Original Two = Two
 ================================================
  !! Expectation Failed: Two should not be Two
 ================================================
```
