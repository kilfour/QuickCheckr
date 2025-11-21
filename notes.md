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
 Test:                    ExampleTest
 Location:                Notes\A_Shrinking\A_ExecutionShrinking\ExecutionShrinking.cs:62:1
 Original failing run:    3 executions
 Minimal failing case:    1 execution (after 2 shrinks)
 Seed:                    669391953
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
    from actOne in Checkr.Act("Act One", () => { })
    from actTwo in Checkr.Act("Act Two", () => { })
    from actThree in Checkr.Act("Act Three", () => throw new Exception("BOOM"))
    select Case.Closed;
```

Execution 1 - Action 1: noop  
Execution 1 - Action 2: noop  
Execution 1 - Action 3: Throws => (Fails)  

So after Action Shrinking, the result is:
  
```text
------------------------------------------------------------
 Test:                    ExampleTest
 Location:                Notes\A_Shrinking\B_ActionShrinking\ActionShrinking.cs:62:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 2 shrinks)
 Seed:                    699025678
 ------------------------------------------------------------
  Executed (1): Act Three
 ===========================================================================
  !! Exception Thrown: System.Exception: BOOM
   at QuickCheckr.Tests.Notes.A_Shrinking.B_ActionShrinking.ActionShrinking.<>c.<Example_Check>b__2_6() in Notes\A_Shrinking\B_ActionShrinking\ActionShrinking.cs:line 75
   at QuickCheckr.Checkr.<>c__DisplayClass1_0.<Act>b__0() in C:\Code\QuickCheckr\QuickCheckr\CheckrAct.cs:line 11
   at QuickCheckr.Checkr.<>c__DisplayClass2_0`1.<TryCatch>b__0(CheckrState state) in C:\Code\QuickCheckr\QuickCheckr\CheckrAct.cs:line 21
 ===========================================================================
```
### Input Shrinking
This is the third step in QuickCheckr shrinking and the most complex one.  
QuickCheckr shrinking tries to remove inputs, one by one, to find a minimal failing case.  
The following simple example has one input but always fails.  
```csharp
    from irrelevant in Checkr.Input("Irrelevant Input", Fuzzr.Int())
    from spec in Checkr.Spec("Always Fails", () => false)
    select Case.Closed;
```
Input shrinking reruns the failing execution and prunes inputs whose values don't affect the failure.

So, in the previous example, after Input Shrinking, the result is:
  
```text
------------------------------------------------------------
 Test:                    ExampleTest
 Location:                Notes\A_Shrinking\C_InputShrinking\InputShrinking.cs:53:1
 Original failing run:    1 execution
 Seed:                    165641598
 ------------------------------------------------------------
  Executed (1)
 ================================
  !! Spec Failed: Always Fails
 ================================
```
Another example, slightly trickier.  
```csharp
    from one in Checkr.Input("Input One", Fuzzr.Int())
    from two in Checkr.Input("Input Two", Fuzzr.Int())
    from three in Checkr.Input("Input Three", Fuzzr.Int())
    from result in Checkr.Act("Result", () => one + three)
    from spec in Checkr.Spec("Should not be 42", () => result != 42)
    select Case.Closed;
```
As you can see from the test definition, two inputs are involved in a possible failure.

QuickCheckr correctly identifies the relevant inputs.

Result:
  
```text
------------------------------------------------------------
 Test:                    Example_Two_Test
 Location:                Notes\A_Shrinking\C_InputShrinking\InputShrinking.cs:110:1
 Original failing run:    1 execution
 Seed:                    231122398
 ------------------------------------------------------------
  Executed (1): Result
   - Input: Input One = 12
   - Input: Input Three = 30
 ====================================
  !! Spec Failed: Should not be 42
 ====================================
```
