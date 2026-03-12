# Checkr Act Carefully When

**The Checkr:**  
```csharp
from counter in Checkr.Input("Counter", Fuzzr.Counter("key"))
from thing in Checkr.ActCarefullyWhen("Act", () => counter != 2, () => counter == 3 ? null : new Thing())
from notNull in Checkr.Expect("Not Null", () => thing.HasValue)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                CheckrActCarefullyWhen.cs:50:1
 Original failing run:    3 executions
 Minimal failing case:    1 execution (after 2 shrinks)
 Seed:                    625676917
 ------------------------------------------------------------
  Executed: Act
   - Input: Counter = 3
 ===================================
  !! Expectation Failed: Not Null
 ===================================
 Passed Expectations
 - Not Null: 1x
 ------------------------------------------------------------
```
