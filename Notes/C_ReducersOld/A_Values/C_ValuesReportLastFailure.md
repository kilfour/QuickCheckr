# Values Report Last Failure

**The Checkr:**  
```csharp
from input in Checkr.Input("Input", Fuzzr.Int(),
   Shrink.OnType<int>(s => s.Simplify(Reduce.Values([3, 0, 1, 2], true))))
from expect in Checkr.Expect("Input < 2", () => input < 2)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                C_ValuesReportLastFailure.cs:59:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    1681332589
 ------------------------------------------------------------
  Executed:
   - Input: Input = 79
            Redux   2
 ====================================
  !! Expectation Failed: Input < 2
 ====================================
```
