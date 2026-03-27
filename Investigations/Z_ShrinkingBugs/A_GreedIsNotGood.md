# Greed Is Not Good
Fixed this one by using Testimonys, see Memory.  

**The Checkr:**  
```csharp
from input in Checkr.Input("List", Fuzzr.Constant(new List<int>() { 1, 1 }))
from length in Checkr.Expect("input.Count == 2", () => input.Count == 2)
from expect in Checkr.ExpectWhen("does not contain 1", () => input.Count == 2, () => !input.Contains(1))
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                A_GreedIsNotGood.cs:40:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    2130064221
 ------------------------------------------------------------
  Executed:
   - Input: List = [ _, 1 ]
 =============================================
  !! Expectation Failed: does not contain 1
 =============================================
 Passed Expectations
 - input.Count == 2: 1x
 ------------------------------------------------------------
```
