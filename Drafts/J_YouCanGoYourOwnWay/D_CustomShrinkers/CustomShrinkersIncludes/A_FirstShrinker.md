# First Shrinker

**The Checkr:**  
```csharp
from answers in Checkr.Input("answer", Fuzzr.Int(),
    Shrink.OnType<int>(
        s => s.Classify(Try.Values(42)),
        s => s.Simplify(Reduce.Towards(100))))
from trace in Checkr.Trace("original", () => answers)
from expect in Checkr.Expect("all answers equal 42", () => answers == 42)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                A_FirstShrinker.cs:32:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    1895340861
 ------------------------------------------------------------
  Executed:
   - Input: answer   = 53
            Redux      100
   - Trace: original = 53
 ===============================================
  !! Expectation Failed: all answers equal 42
 ===============================================
```
