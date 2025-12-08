# On Shrinking
Whenever QuickCheckr comes accross a failure during a run, 
it tries to shrink the failing case to find a minimal failing case.

During this process no further executions are performed and no new random values are generated.

QuickCheckr then reports the minimal failing case, 
together with the number of executions, relevant actions and inputs that were required to find it.  
## Philosophy

QuickCheckr keeps shrinking simple and predictable.  
It never tries to *outsmart* your domain with clever global algorithms.  
Shrinking is purely local and irrelevance-based: if removing a value (or execution) still makes the property fail,
QuickCheckr marks it as *irrelevant*. 
This avoids surprising or *magical* shrink results.  
In rare circumstances (for instance, symmetric cases where everything collapses),
you can guide the process by configuring a *stand-in* value (e.g. Trackr.Substitute<int>(-1)),
which helps QuickCheckr distinguish roles without adding complexity.
  
## Execution Shrinking
This is the first step in QuickCheckr shrinking.  
The following simple example fails on the third execution.  
```csharp
    from counter in Checkr.Input("counter", Fuzzr.Counter("counter"))
    from spec in Checkr.Spec("Counter does not equal 3", () => counter != 3)
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
 Test:                    ExampleTest
 Location:                Docs\H_OnShrinking\A_ExecutionShrinking\ExecutionShrinking.cs:53:1
 Original failing run:    3 executions
 Minimal failing case:    1 execution (after 2 shrinks)
 Seed:                    669391953
 ------------------------------------------------------------
  Executed:
   - Input: counter = 3
 ============================================
  !! Spec Failed: Counter does not equal 3
 ============================================
 Passed Specs
 - Counter does not equal 3: 2x
 ------------------------------------------------------------
```
## Input Shrinking
This is the next step in QuickCheckr shrinking and the most complex one.  
QuickCheckr shrinking tries to remove inputs, one by one, to find a minimal failing case.  
The following simple example has one input but always fails.  
```csharp
    from irrelevant in Checkr.Input("Irrelevant Input", Fuzzr.Int())
    from spec in Checkr.Spec("Always Fails", () => false)
    select Case.Closed;
```
Input shrinking reruns the failing execution and prunes inputs whose values don't affect the failure.

So, in the previous example, after *Input Shrinking*, the result is:
  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Notes\A_Shrinking\C_InputShrinking\InputShrinking.cs:27:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    165641598
 ------------------------------------------------------------
  Executed
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
 Test:                    Example_Two
 Location:                Notes\A_Shrinking\C_InputShrinking\InputShrinking.cs:65:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    231122398
 ------------------------------------------------------------
  Executed: Result
   - Input: Input One = 12
   - Input: Input Three = 30
 ====================================
  !! Spec Failed: Should not be 42
 ====================================
```
### Enums
```csharp
    from inputOne in Checkr.Input("Input One", Fuzzr.Enum<DayOfWeek>())
    from inputTwo in Checkr.Input("Input Two", Fuzzr.Enum<DayOfWeek>())
    from spec in Checkr.Spec("Should not be Monday", () => inputTwo != DayOfWeek.Monday)
    select Case.Closed;
```
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Notes\A_Shrinking\C_InputShrinking\A_Primitives\EnumShrinkingTests.cs:19:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    1732064051
 ------------------------------------------------------------
  Executed:
   - Input: Input Two = Monday
 ========================================
  !! Spec Failed: Should not be Monday
 ========================================
```
```csharp
    from inputOne in Checkr.Input("Input One", Fuzzr.Enum<SoundCheck>())
    from inputTwo in Checkr.Input("Input Two", Fuzzr.Enum<SoundCheck>())
    from spec in Checkr.Spec("Should not be Two", () => inputTwo != SoundCheck.Two)
    select Case.Closed;
```
```text
------------------------------------------------------------
 Test:                    RunTest_Custom
 Location:                Notes\A_Shrinking\C_InputShrinking\A_Primitives\EnumShrinkingTests.cs:55:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    1205305161
 ------------------------------------------------------------
  Executed:
   - Input: Input Two = Two
 =====================================
  !! Spec Failed: Should not be Two
 =====================================
```
### Compendium
Here you'll find a collection of examples of input shrinking.  
These can be used as a starting point for your own tests.
  
