# Values Stop On First Passed Alternative

**The Checkr:**  
```csharp
from input in Checkr.Input("Input", Fuzzr.Int(), Shrink.OnType(Reduce.Values([3, 0, 1, 2])))
from expect in Checkr.Expect("Input < 2", () => input < 2)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                B_ValuesStopOnFirstPassedAlternative.cs:59:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    1681332589
 ------------------------------------------------------------
  Executed:
   - Input: Input = 79
            Redux   3
 ====================================
  !! Expectation Failed: Input < 2
 ====================================
```
