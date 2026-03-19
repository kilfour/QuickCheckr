# Execution Shrinking
[On Shrinking][OnShrinking] | [Next][InputShrinking]  
  
This is the first step in QuickCheckr shrinking.  
The following simple example fails on the third execution.  
```csharp
from counter in Checkr.Input("counter", Fuzzr.Counter("counter"))
from expect in Checkr.Expect("Counter does not equal 3", () => counter != 3)
select Case.Closed;
```

Execution 1: counter = 1  
Execution 2: counter = 2  
Execution 3: counter = 3  (Fails)  

During execution shrinking the values from counter are not regenerated, but retrieved from the previous run.
So the ExecutionShrinker retries the run with the same values, but tries to remove executions, one by one.  
If the run still fails, the execution is not deemed relevant for the failure and is removed.  

So after *Execution Shrinking*, the result is:
  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                ExecutionShrinking.cs:68:1
 Original failing run:    3 executions
 Minimal failing case:    1 execution (after 2 shrinks)
 Seed:                    669391953
 ------------------------------------------------------------
  Executed:
   - Input: counter = 3
 ===================================================
  !! Expectation Failed: Counter does not equal 3
 ===================================================
 Passed Expectations
 - Counter does not equal 3: 2x
 ------------------------------------------------------------
```

[OnShrinking]: ../OnShrinking.md

[InputShrinking]: ../C_InputShrinking/InputShrinking.md
