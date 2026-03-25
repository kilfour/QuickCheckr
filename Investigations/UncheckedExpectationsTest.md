# Unchecked Expectations

**The Checkr:**  
```csharp
from input in Checkr.Input("input", Fuzzr.Counter("count"))
from expect in Checkr.ExpectWhen("checked", () => input > 1, () => false)
from expectUnchecked in Checkr.ExpectWhen("unchecked", () => input > 2, () => false)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                UncheckedExpectationsTest.cs:40:1
 Original failing run:    2 executions
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    96544855
 ------------------------------------------------------------
  Executed:
   - Input: input = 2
 ==================================
  !! Expectation Failed: checked
 ==================================
 ! Warning: Unchecked Expectations !
 - unchecked
 ------------------------------------------------------------
```
