# Chained

**The Checkr:**  
```csharp
from input in Checkr.Input("Input", Fuzzr.Int(),
   Shrink.OnType<int>(s => s.Simplify(Reduce.Towards(5), Reduce.Towards(0))))
from expect in Checkr.Expect("Input < 2", () => input < 2)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                A_Chained.cs:32:1
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
