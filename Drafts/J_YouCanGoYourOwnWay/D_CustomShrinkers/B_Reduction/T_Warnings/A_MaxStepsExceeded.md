# Max Steps Exceeded

**The Checkr:**  
```csharp
from input in Checkr.Input("Input", Fuzzr.Constant(42))
from expect in Checkr.Expect("Input <= 0", () => input <= 0)
select Case.Closed;
```

**The Configuration:**  
```csharp
config with { MaxStepsPerReducer = 10, ShrinkMode = config.ShrinkMode | ShrinkMode.Reduction };
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                A_MaxStepsExceeded.cs:24:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    1719639579
 ------------------------------------------------------------
  Executed:
   - Input: Input = 42
            Redux   30
   - WARNING: Reducer stopped at Input. Max steps (10) exceeded.
 =====================================
  !! Expectation Failed: Input <= 0
 =====================================
```
