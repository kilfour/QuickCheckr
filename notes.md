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
 Location:                Notes\A_Shrinking\A_ExecutionShrinking\ExecutionShrinking.cs:62:1
 Original failing run:    3 executions
 Minimal failing case:    1 execution (after 2 shrinks)
 Seed:                    669391953
 ------------------------------------------------------------
  Executed (3):
   - Input: counter = 3
 ============================================
  !! Spec Failed: Counter does not equal 3
 ============================================
```
### Action Shrinking
This is the second step in QuickCheckr shrinking, but it needs to be explicitely activated
by passing in a configuration function to the Run method: `a => a with { ShrinkingActions = true }`.  

It is not common to have multiple Actions in one execution, but it is possible, and sometimes useful.  

In the same vein as with *Execution Shrinking*, Actions are also evaluated and judged to be relevant for the failure.  
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

So after *Action Shrinking*, the result is:
  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Notes\A_Shrinking\B_ActionShrinking\ActionShrinking.cs:32:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 2 shrinks)
 Seed:                    699025678
 ------------------------------------------------------------
  Executed (1): Act Three
 ===========================================================================
  !! Exception Thrown: System.Exception: BOOM
   at QuickCheckr.Tests.Notes.A_Shrinking.B_ActionShrinking.ActionShrinking.<>c.<GetCheckr>b__2_6() in Notes\A_Shrinking\B_ActionShrinking\ActionShrinking.cs:line 55
   at QuickCheckr.Checkr.<>c__DisplayClass1_0.<Act>b__0() in CheckrAct.cs:line 12
   at QuickCheckr.Checkr.<>c__DisplayClass2_0`1.<TryCatch>b__0(CheckrState state) in CheckrAct.cs:line 22
 ===========================================================================
```
#### Elaborating
The reason *Action Shrinking* is not activated by default is because in some rare cases it can lead to confusion.  
Consider the following example:  
```csharp
    from act in Checkr.Act("Act", () => false)
    from spec in Checkr.Spec("Act s true", () => act == true)
    select Case.Closed;
```
If we enable *Action Shrinking* QuickCheckr replays the run and deactivates the action to ascertain its relevance.  
However, the action returns a value which other `LINQ` operations might depend on, so QuickCheckr returns a `default` value.  

But, and here's the confusing bit, the spec in our example fails if the action returns false, the default bool value,
so the spec fails even if the action is deactivated, and hence QuickCheckr decides it is irrelevant and happily removes it from the run.  

And here we find ourselves confused ;-).  
  
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

So, in the previous example, after *Input Shrinking*, the result is:
  
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
#### Collections
##### Fails On Size
```csharp
    from listInput in Checkr.Input("List Input", Fuzzr.Int().Many(0, 10))
    from spec in Checkr.Spec("List does not contain 7", () => !listInput.Contains(7))
    from trace in Checkr.Trace("List Input", listInput)
    select Case.Closed;
```
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Notes\A_Shrinking\C_InputShrinking\B_Collections\TheExamples\A_FailsOnSize.cs:18:1
 Original failing run:    1 execution
 Seed:                    1833095082
 ------------------------------------------------------------
  Executed (1):
   - Input: List Input = [ 7 ]
   - Trace: List Input = [ 69, 49, 90, 36, 16, 71, 95, 7, 84 ]
 ===========================================
  !! Spec Failed: List does not contain 7
 ===========================================
```
## Building Blocks
### Checkr Input
## Examples
### The Element Remover
This test focuses on a simple faulty implementation:  
```csharp
public static class ElementRemover
{
    public static List<int> Remove(List<int> list, int item)
    {
        list.Remove(item);
        return list;
    }
}
```
`ElementRemover` only removes the *first* occurrence of a value in a list.  
Using one input, one action, and one specification,
the test captures the minimal failing case where the list contains multiple copies of the target element.  
```csharp
    from list in Checkr.Input("List", GetFuzzr())
    from element in Checkr.Input("Item to Remove", Fuzzr.Int(0, 10))
    from output in Checkr.Act("Delete from List",
        () => ElementRemover.Remove([.. list], element))
    from listDoesNotContainElement in Checkr.Spec(
        "List Does Not Contain Element",
        () => !output.Contains(element))
    from traceList in Checkr.Trace("List Before Shrinking", list)
    select Case.Closed;
```
QuickAcid reduces the failing run to a list like `[ 5, 5 ]` with `5` as the element to remove,
exposing the bug clearly: the output still contains `5`.  
The generated report shows the input, the attempted deletion, a trace of the original list before shrinking and a trace of the result from the action.  
  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Notes\T_Examples\A_TheElementRemover\TheElementRemover.cs:31:1
 Original failing run:    1 execution
 Seed:                    438724296
 ------------------------------------------------------------
  Executed (1): Delete from List
   - Input: List = [ 5, 5 ]
   - Input: Item to Remove = 5
   - Trace: List Before Shrinking = [ 2, 8, 0, 5, 5, 1, 8, 2, 9 ]
 =================================================
  !! Spec Failed: List Does Not Contain Element
 =================================================
```
### Bugging The House
This is a set of intentionally fragile programs, each with its own peculiar way of failing.  
They range from simple state-based traps to intricate multi-method booby traps.  
#### Delayed Detonation
```csharp
public class BugHouse
{
    private int count;
    public bool Run(int a)
    {
        return !(count++ == 2 && a == 1);
    }
}
```
Throws only after exactly three total runs when the input value is 1.  
Demonstrates a simple stateful failure triggered by run count.  
```csharp
    from a in Checkr.Input("a", Fuzzr.Int())
    from bughouse in Trackr.Stashed(() => new BugHouse())
    from output in Checkr.Act("BugHouse.Run", () => bughouse.Run(a))
    from spec in Checkr.Spec("Returns true", () => output)
    select Case.Closed;
```
##### Minimal Fail Case:
- **Length:** 3 executions.
- **Example input:** `[42, 666, 1]`
```csharp
count = 0
Run( a = <any> ) // count = 1
Run( a = <any> ) // count = 2
Run( a = 1 ) // count == 2 && a == 1 → fail
```  
##### Report:
```text
------------------------------------------------------------
 Test:                    <RunTest>b__1_0
 Location:                Notes\T_Examples\B_BuggingTheHouse\Bugs\A_DelayedDetonation.cs:42:1
 Original failing run:    3 executions
 Seed:                    1914853878
 ------------------------------------------------------------
  Executed : BugHouse.Run (2 Times)
 ------------------------------------------------------------
  Executed (3): BugHouse.Run
   - Input: a = 1
 ================================
  !! Spec Failed: Returns true
 ================================
```
