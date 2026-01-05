# Shrink Multiple

**The Checkr:**  
```csharp
from counter in Checkr.Input("counter", Fuzzr.Counter("counter"))
from thingOne in Checkr.Input("Null Thing One",
    Fuzzr.Constant<Thing>(null!),
    // The second one will remove this value from the report.
    // The first one will still emit a warning.
    () => [Shrink.Default<Thing>(), Shrink.Values(new Thing())])
from thingTwo in Checkr.Input("Null Thing Two",
    Fuzzr.Constant<Thing>(null!),
    // Now the first one will shortcircuit shrinking, so no warning from the second one.
    () => [Shrink.Values(new Thing()), Shrink.Default<Thing>()])
from expect in Checkr.Expect("counter != 2", () => counter != 2)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                ShrinkMultiple.cs:57:1
 Original failing run:    2 executions
 Minimal failing case:    1 execution (after 3 shrinks)
 Seed:                    161541321
 ------------------------------------------------------------
  Executed:
   - Input: counter = 2
   - WARNING: Null value for type 'Thing' at input 'Null Thing One'.
 =======================================
  !! Expectation Failed: counter != 2
 =======================================
 Passed Expectations
 - counter != 2: 1x
 ------------------------------------------------------------
```
