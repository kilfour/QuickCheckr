# Shrink None

**The Checkr:**  
```csharp
from counter in Checkr.Input("counter", Fuzzr.Counter("counter"))
from thing in Checkr.Input("Null Thing",
    Fuzzr.Constant<Thing>(null!),
    () => Shrink.None<Thing>())
from expect in Checkr.Expect("counter != 2", () => counter != 2)
select Case.Closed;
```
Weird option this one at first glance, maybe. It does not make inputs turn up in the report.  
We know it works because of the shrink count.
No shrinking => no shrinking clue => not in report.
This is correct behavior.
  

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                ShrinkNone.cs:67:1
 Original failing run:    2 executions
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    161541321
 ------------------------------------------------------------
  Executed:
   - Input: counter = 2
 =======================================
  !! Expectation Failed: counter != 2
 =======================================
 Passed Expectations
 - counter != 2: 1x
 ------------------------------------------------------------
```
