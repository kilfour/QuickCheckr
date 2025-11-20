# Notes
A temporary collection of documented tests to be moved into the documentation later.  
## Shrinking
Whenever QuickCheckr comes accross a failure during a run, 
it tries to shrink the failing case to find a minimal failing case.

During this process no further executions are performed and no new random values are generated.

QuickCheckr then reports the minimal failing case, 
together with the number of executions, relevant actions and inputs that were required to find it.  
### Execution Shrinking
This is the first step in QuickCheckr shrinking.  
The following simple example fails on the third execution.  
```csharp
    from counter in Checkr.Input("counter", Fuzzr.Counter("counter"))
    from spec in Checkr.Spec("Fails on Three", () => counter != 3)
    select Case.Closed;
```

Execution 1: counter = 1  
Execution 2: counter = 2  
Execution 3: counter = 3  (Fails)  

During execution shrinking the values from counter are not regenerated, but retrieved from the previous run.
So the ExecutionShrinker retries the run with the same values, but tries to remove executions, one by one.  
If the run still fails, the execution is not deemed relevant for the failure and is removed.  

So after Execution Shrinking, the result is:
  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                ExecutionShrinking.cs:28:1
 Original failing run:    3 executions
 Minimal failing case:    1 execution (after 2 shrinks)
 Seed:                    1464227412
 ------------------------------------------------------------
  Executed (3):
   - Input: counter = 3
 ==================================
  !! Spec Failed: Fails on Three
 ==================================
```
### Action Shrinking
This is the second step in QuickCheckr shrinking.  
It is not common to have multiple Actions in one execution, but it is possible, and sometimes useful.  
In the same vein as with Execution Shrinking, Actions are also reevaluated and judged to be relevant for the failure.  
The following simple example fails on the third action.  
```csharp
    from counter in Checkr.Input("counter", Fuzzr.Counter("counter"))
    from spec in Checkr.Spec("Fails on Three", () => counter != 3)
    select Case.Closed;
```

Execution 1 - Action 1: noop
Execution 1 - Action 2: noop  
Execution 1 - Action 3: Throws => (Fails)  

So after Action Shrinking, the result is:
  
```text
------------------------------------------------------------
```
