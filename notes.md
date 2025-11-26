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
 Location:                Notes\A_Shrinking\A_ExecutionShrinking\ExecutionShrinking.cs:54:1
 Original failing run:    3 executions
 Minimal failing case:    1 execution (after 2 shrinks)
 Seed:                    669391953
 ------------------------------------------------------------
  Executed (3):
   - Input: counter = 3
 ============================================
  !! Spec Failed: Counter does not equal 3
 ============================================
 Passed Specs
 - Counter does not equal 3: 2x
 ------------------------------------------------------------
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
#### Enums
```csharp
    from inputOne in Checkr.Input("Input One", Fuzzr.Enum<DayOfWeek>())
    from inputTwo in Checkr.Input("Input Two", Fuzzr.Enum<DayOfWeek>())
    from spec in Checkr.Spec("Should not be Monday", () => inputTwo != DayOfWeek.Monday)
    select Case.Closed;
```
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Notes\A_Shrinking\C_InputShrinking\A_Primitives\EnumShrinkingTests.cs:20:1
 Original failing run:    1 execution
 Seed:                    1732064051
 ------------------------------------------------------------
  Executed (1):
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
 Seed:                    1205305161
 ------------------------------------------------------------
  Executed (1):
   - Input: Input Two = Two
 =====================================
  !! Spec Failed: Should not be Two
 =====================================
```
#### Collections
##### Removing Irrelevant Items
When shrinking collections, QuickCheckr does not attempt to rewrite or regenerate the list.
Instead, it removes items one by one, replaying the run each time to test whether the removed element was relevant for the failure.

This means that if only some elements in a list contribute to the failing condition, QuickCheckr will aggressively trim the rest away. The result is a minimal list containing only the values that still trigger the failure, even if the original list was large or noisy.

Consider a test that forbids the value `7`:  
```csharp
    from listInput in Checkr.Input("List Input", Fuzzr.Int().Many(0, 10))
    from spec in Checkr.Spec("List does not contain 7", () => !listInput.Contains(7))
    from trace in Checkr.Trace("List Input", () => listInput)
    select Case.Closed;
```
Even if the original list was long and varied, QuickCheckr isolates the single relevant element:  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Notes\A_Shrinking\C_InputShrinking\B_Collections\TheExamples\A_RemovingIrrelevantItems.cs:26:1
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
##### Irrelevant Element But List Length Is Not
When shrinking collections, QuickCheckr isolates the structural aspects that contributed to the failure
and leaves irrelevant details untouched. 

In cases like this example where the predicate depends solely on the length of the list,
the individual element values play no role.  
QuickCheckr therefore preserves them as anonymous placeholders (_) and concentrates entirely on reducing the collection's shape:
removing elements, and converging toward the smallest list that still violates the condition.

This yields minimal, focused shrinking artifacts:
the report shows only the structural signal that mattered (here: the list being *too long*),
without introducing noise from element-level changes that could not affect the outcome anyway.  
```csharp
    from length in Checkr.Input("Length", Fuzzr.Int(0, 10))
    from list in Checkr.Input("List", Fuzzr.Int().Many(1, 10))
    from trace in Checkr.Trace("Original List", () => list)
    from spec in Checkr.Spec("Length <= 2", () => list.Count() <= 2)
    select Case.Closed;
```
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Notes\A_Shrinking\C_InputShrinking\B_Collections\TheExamples\C_IrrelevantElementButListLengthIsNot.cs:30:1
 Original failing run:    1 execution
 Seed:                    1994546166
 ------------------------------------------------------------
  Executed (1):
   - Input: List = [ _, _, _ ]
   - Trace: Original List = [ 26, 42, 46, 35, 23, 62 ]
 ===============================
  !! Spec Failed: Length <= 2
 ===============================
```
##### Relevant Multiple Eelements
```csharp
    from input in Checkr.Input("input", Fuzzr.Int(1, 10).Many(1, 10))
    from spec in Checkr.SpecIf("spec", () => input.Count() > 2, () =>
        !(input.ToList()[0] == 1
        && input.ToList()[1] == 2
        && input.ToList()[2] == 3))
    select Case.Closed;
```
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Notes\A_Shrinking\C_InputShrinking\B_Collections\TheExamples\D_RelevantMultipleEelements.cs:19:1
 Original failing run:    1 execution
 Seed:                    500957855
 ------------------------------------------------------------
  Executed (1):
   - Input: input = [ 1, 2, 3 ]
 ========================
  !! Spec Failed: spec
 ========================
```
##### Relevant Element And Size
```csharp
    from input in Checkr.Input("List", Fuzzr.Constant<IEnumerable<int>>([666, 42]))
    from spec in Checkr.SpecIf("Second element is not 42", () => input.Count() > 1, () => input.ElementAt(1) != 42)
    select Case.Closed;
```
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Notes\A_Shrinking\C_InputShrinking\B_Collections\TheExamples\E_RelevantElementAndSize.cs:19:1
 Original failing run:    1 execution
 Seed:                    1058309953
 ------------------------------------------------------------
  Executed (1):
   - Input: List = [ _, 42 ]
 ============================================
  !! Spec Failed: Second element is not 42
 ============================================
```
##### Nested Collections
```csharp
    from input in Checkr.Input("List", Fuzzr.Int().Many(0, 3).Many(0, 3))
    from trace in Checkr.Trace("List", () => input)
    from spec in Checkr.SpecIf("List[0][0] is not 42",
        () => input.Any() && input.First().Any(),
        () => input.First().First() != 42)
    select Case.Closed;
```
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Notes\A_Shrinking\C_InputShrinking\B_Collections\TheExamples\F_NestedCollections.cs:19:1
 Original failing run:    1 execution
 Seed:                    146712596
 ------------------------------------------------------------
  Executed (1):
   - Input: List = [ [ 42 ] ]
   - Trace: List = [ [ 42, 1, 15 ], [ 17 ], [ 27, 9 ] ]
 ========================================
  !! Spec Failed: List[0][0] is not 42
 ========================================
```
#### Custom Shrinkers
```csharp
    from one in Checkr.Input("One", Fuzzr.Int(), a => a with { Shrinker = b => [0, 1] })
    from two in Checkr.Input("Two", Fuzzr.Int(), a => a with { Shrinker = b => [0, 1] })
    from traceOne in Checkr.Trace("Original One", () => one)
    from traceTwo in Checkr.Trace("Original Two", () => two)
    from spec in Checkr.Spec("One should not be 42", () => one != 42)
    select Case.Closed;
```
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Notes\A_Shrinking\C_InputShrinking\T_CustomShrinkers\CustomShrinkers.cs:19:1
 Original failing run:    1 execution
 Seed:                    2122063655
 ------------------------------------------------------------
  Executed (1):
   - Input: One = 42
   - Trace: Original One = 42
   - Trace: Original Two = 90
 ========================================
  !! Spec Failed: One should not be 42
 ========================================
```
#### Fuzzr Shrinking
```csharp
    from one in Checkr.Input("One", Fuzzr.Int(), a => a with { FuzzrShrinker = 3 })
    from two in Checkr.Input("Two", Fuzzr.Int(), a => a with { FuzzrShrinker = 3 })
    from traceOne in Checkr.Trace("Original One", () => one)
    from traceTwo in Checkr.Trace("Original Two", () => two)
    from spec in Checkr.Spec("One should not be 42", () => one != 42)
    select Case.Closed;
```
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Notes\A_Shrinking\C_InputShrinking\T_CustomShrinkers\FuzzrShrinking.cs:19:1
 Original failing run:    1 execution
 Seed:                    2122063655
 ------------------------------------------------------------
  Executed (1):
   - Input: One = 42
   - Trace: Original One = 42
   - Trace: Original Two = 90
 ========================================
  !! Spec Failed: One should not be 42
 ========================================
```
#### Input Reduction
```csharp
    from i in Checkr.Input("Int", Fuzzr.Int(), a => a with { Reducer = TowardsZero })
    from spec in Checkr.Spec("Int should be smaller than 42", () => i < 42)
    from trace in Checkr.Trace("Int", () => i)
    select Case.Closed;
```
```csharp
private static IEnumerable<int> TowardsZero(int input)
{
    var local = input;
    while (local != 0)
    {
        if (local > 0)
            yield return --local;
        else
            yield return ++local;
    }
}
```
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Notes\A_Shrinking\C_InputShrinking\W_Reducers\InputReduction.cs:20:1
 Original failing run:    1 execution
 Seed:                    1681332589
 ------------------------------------------------------------
  Executed (1):
   - Input: Int = 42
   - Trace: Int = 79
 =================================================
  !! Spec Failed: Int should be smaller than 42
 =================================================
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
    from traceList in Checkr.Trace("List Before Shrinking", () => list)
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
#### Third Times the Harm
```csharp
public class BugHouse
{
    private int count;
    public bool Run(int a)
    {
        if (a == 6) count++;
        return !(count == 3);
    }
}
```
Throws as soon as the value 6 has been seen three times.  
A straightforward counter-based failure condition.  
```csharp
    from a in Checkr.Input("a", Fuzzr.Int())
    from bughouse in Trackr.Stashed(() => new BugHouse())
    from output in Checkr.Act("BugHouse.Run", () => bughouse.Run(a))
    from spec in Checkr.Spec("Returns true", () => output)
    select Case.Closed;
```
##### Minimal Fail Case:
- **Length:** 3 executions.
- **Example input:** `[6, 6, 6]`
```csharp
count = 0
Run( a = 6 ) // count = 1
Run( a = 6 ) // count = 2
Run( a = 6 ) // count == 3 → fail
```  
##### Report:
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Notes\T_Examples\B_BuggingTheHouse\Bugs\B_ThirdTimesTheHarm.cs:34:1
 Original failing run:    36 executions
 Minimal failing case:    3 executions (after 33 shrinks)
 Seed:                    766917706
 ------------------------------------------------------------
  Executed : BugHouse.Run (3 Times)
   - Input: a = 6
 ================================
  !! Spec Failed: Returns true
 ================================
 Passed Specs
 - Returns true: 35x
 ------------------------------------------------------------
```
#### Convoluted Countdown
```csharp
public class BugHouse
{
	private int count;
	public bool Run(int a)
	{
		if (a == 6 && count != 3) count++;
		if (count >= 3) count++;
		if (count == 5) return false; return true;
	}
}
```
Fails after a more complex sequence: specific inputs increment the counter in different ways, 
requiring a particular input dance to trigger the failure.  
```csharp
from a in Checkr.Input("a", Fuzzr.Int())
from bughouse in Trackr.Stashed(() => new BugHouse())
from output in Checkr.Act("BugHouse.Run", () => bughouse.Run(a))
from spec in Checkr.Spec("Returns true", () => output)
select Case.Closed;
```
##### Minimal Fail Case:
- **Length:** 4 executions.
- **Example input:** `[6, 6, 6, 42]`
```csharp
count = 0
Run( a = 6 ) // +1 → 1
Run( a = 6 ) // +1 → 2
Run( a = 6 ) // +1 → 3, then >=3 → +1 → 4
Run( a != 6 ) // skip first inc, >=3 → +1 → 5 → fail
```  
##### Reports:
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Notes\T_Examples\B_BuggingTheHouse\Bugs\B_ThirdTimesTheHarm.cs:34:1
 Original failing run:    36 executions
 Minimal failing case:    3 executions (after 33 shrinks)
 Seed:                    766917706
 ------------------------------------------------------------
  Executed : BugHouse.Run (3 Times)
   - Input: a = 6
 ================================
  !! Spec Failed: Returns true
 ================================
 Passed Specs
 - Returns true: 35x
 ------------------------------------------------------------
```
#### Multiple Methods
```csharp
public class BugHouse
{
	private string? bug;
	public bool RunInt(int a)
	{
		bug += "1";
		if (bug.EndsWith("1221") && a == 6) return false;
		return true;
	}
	public bool RunString(string a)
	{
		bug += "2";
		if (bug.EndsWith("122") && a == "p") return false;
		return true;
	}
}
```
Exception occurs only from a specific interleaving of RunInt and RunString calls, each appending different markers to state.
Demonstrates multi-method state interaction.  
```csharp
	from bughouse in Trackr.Stashed(() => new BugHouse())
	from ops in Checkr.OneOf(
		// --
		from i in Checkr.Input("Int", Fuzzr.Int())
		from output in Checkr.Act("BugHouse.RunInt", () => bughouse.RunInt(i))
		from spec in Checkr.Spec("RunInt s true", () => output)
		select Case.Closed,
		// --
		from str in Checkr.Input("String", Fuzzr.String())
		from output in Checkr.Act("BugHouse.RunString", () => bughouse.RunString(str))
		from spec in Checkr.Spec("RunString s true", () => output)
		select Case.Closed)
		//--
	select Case.Closed;
```
##### Minimal Fail Case (one example):
- **Length:** 4 executions.
- **Example input sequence:**
```csharp
RunInt(<any>)        // bug = "1"
RunString(<any>)     // bug = "12"
RunString(<any>)     // bug = "122"
RunInt(6)            // bug ends with "1221" and `a == 6` → fail
```  
##### Reports:
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Notes\T_Examples\B_BuggingTheHouse\Bugs\D_MultipleMethods.cs:34:1
 Original failing run:    59 executions
 Minimal failing case:    4 executions (after 55 shrinks)
 Seed:                    1292206393
 ------------------------------------------------------------
  Executed (56): BugHouse.RunInt
 ------------------------------------------------------------
  Executed : BugHouse.RunString (2 Times)
 ------------------------------------------------------------
  Executed (59): BugHouse.RunInt
   - Input: Int = 6
 =======================================
  !! Spec Failed: RunInt returns true
 =======================================
 Passed Specs
 - RunString returns true: 34x
 - RunInt returns true: 24x
 ------------------------------------------------------------
```
### There Is Always An Account
#### The Model:
```csharp
public class Account
{
    public int Balance { get; private set; }
    public void Deposit(int amount) => Balance += amount;
    public void WithDraw(int amount) => Balance -= amount;
}
```
#### The Checkr:
```csharp
    from account in Trackr.Stashed(() => new Account())
    from amount in Checkr.Input("Amount", Fuzzr.Int(), a => a with { Reducer = TowardsZero })
    from trace in Checkr.Trace("Amount", () => amount)
    from operations in Checkr.OneOf(
        Checkr.Act("Deposit", () => account.Deposit(amount)),
        Checkr.Act("Withdraw", () => account.WithDraw(amount))
    )
    from noOverdraft in Checkr.Spec("No Overdraft", () => account.Balance >= 0)
    select Case.Closed;
```
#### The Report:
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Notes\T_Examples\D_ThereIsAlwaysAnAccount\ThereIsAlwaysAnAccount.cs:25:1
 Original failing run:    5 executions
 Minimal failing case:    1 execution (after 4 shrinks)
 Seed:                    1863050139
 ------------------------------------------------------------
  Executed (5): Withdraw
   - Input: Amount = 1
   - Trace: Amount = 88
 ================================
  !! Spec Failed: No Overdraft
 ================================
 Passed Specs
 - No Overdraft: 4x
 ------------------------------------------------------------
```
#### Original Run:
```text
------------------------------------------------------------
 Test:                    FullRunTest
 Location:                Notes\T_Examples\D_ThereIsAlwaysAnAccount\ThereIsAlwaysAnAccount.cs:50:1
 Original failing run:    5 executions
 Seed:                    1863050139
 ------------------------------------------------------------
  Executed (1): Deposit
   - Trace: Amount = 94
 ------------------------------------------------------------
  Executed (2): Deposit
   - Trace: Amount = 59
 ------------------------------------------------------------
  Executed (3): Withdraw
   - Trace: Amount = 43
 ------------------------------------------------------------
  Executed (4): Withdraw
   - Trace: Amount = 63
 ------------------------------------------------------------
  Executed (5): Withdraw
   - Trace: Amount = 88
 ================================
  !! Spec Failed: No Overdraft
 ================================
 Passed Specs
 - No Overdraft: 4x
 ------------------------------------------------------------
```
### Bolting Horses
For this example we are going the use a simplified version of the [`HorsesForCourses`](https://github.com/kilfour/HorsesForCourses/blob/main/README.md) domain.  
The bug we're hunting was actually found in the wild (although I simplified the code, in order to keep it short)
and is demonstrative for a whole category: *Invariants drifting over time*.
  
**The Coach:**  
```csharp
public class Coach(string name)
{
    public string Name { get; init; } = name;
    public IReadOnlySet<string> Skills => skills;
    private readonly HashSet<string> skills = [];
    public IReadOnlyList<Course> AssignedCourses => assignedCourses;
    private readonly List<Course> assignedCourses = [];
    public void UpdateSkills(IEnumerable<string> newSkills)
    {
        skills.Clear();
        foreach (var s in newSkills) skills.Add(s);
    }
    public bool IsSuitableFor(Course course) => course.RequiredSkills.All(Skills.Contains);
    public void AssignCourse(Course course) => assignedCourses.Add(course);
}
```
**The Course:**  
```csharp
public class Course(string name)
{
    public string Name { get; init; } = name;
    public IReadOnlySet<string> RequiredSkills => requiredSkills;
    private readonly HashSet<string> requiredSkills = [];
    public Coach? AssignedCoach { get; private set; }
    public void UpdateRequiredSkills(IEnumerable<string> newSkills)
    {
        requiredSkills.Clear();
        foreach (var s in newSkills) requiredSkills.Add(s);
    }
    public void AssignCoach(Coach coach)
    {
        if (!coach.IsSuitableFor(this))
            throw new InvalidOperationException("Coach does not have the required skills");
        AssignedCoach = coach;
        coach.AssignCourse(this);
    }
}
```
**The Checkr:**  
This one is the *entry point* for the test.  
It creates two pools, one for coaches, and one for courses, which we are going to use to track the state of the system.  
It then creates a series of actions, randomly choosing one, using `Checkr.OneOfIf`, out of those in the list for which it's predicate is fulfilled.
Finally after each action we check the `AssignedCoachesMustBeSuitable` Invariant.  
```csharp
private static readonly CheckrOf<Case> TheCheckr =
    from coaches in Trackr.Pool<Coach>()
    from courses in Trackr.Pool<Course>()
    from operations in Checkr.OneOfIf(
        (() => coaches.Count() < 10, CreateCoach),
        (() => coaches.Any(), UpdateCoachSkills),
        (() => courses.Count() < 5, CreateCourse),
        (() => courses.Any(), UpdateCourseSkills),
        (() => coaches.Any() && courses.Any(course => course.AssignedCoach == null), AssignCoachToCourse))
    from invariant in AssignedCoachesMustBeSuitable
    select Case.Closed;
```
**Create Coach Checkr:**  
In this one we create a coach with a random name, verify that the property `Name` is assigned and then add it to the coaches pool.  
```csharp
private static readonly CheckrOf<Case> CreateCoach =
    from name in Checkr.Input("Coach Name",
        from cnt in Fuzzr.Counter("coach-name") select $"Coach-{cnt}")
    from trace in Checkr.Trace("Coach Name", () => name)
    from coach in Checkr.Act("Create Coach", () => new Coach(name))
    from checkName in Checkr.Spec("Coach Name is set", () => coach.Name == name)
    from toPool in Trackr.Pool("Store Coach", coach)
    select Case.Closed;
```
**Update Coach Skills Checkr:**  
Now we retrieve a random coach from the pool and update its skills, verifying that the property `Skills` is set correctly.  
```csharp
private static readonly CheckrOf<Case> UpdateCoachSkills =
    from coach in Trackr.OneOfPool<Coach>("Coach to update")
    from skills in Checkr.Input("Coach Skills", Fuzzr.OneOf(SkillPool).Unique(Guid.NewGuid()).Many(3, 5))
    from valid in Checkr.When(() => coach is not null,
        from trace in Checkr.Trace("Coach Name", () => coach.Name)
        from updateSkills in Checkr.Act("Update Coach Skills",
            () => coach.UpdateSkills(skills))
        from checkSkills in Checkr.Spec("Coach Skills Set",
            () => coach.Skills.Count == skills.Count() && coach.Skills.All(skills.Contains))
        select Case.Closed)
    select Case.Closed;
```
**Create Course Checkr:**  
Same pattern as the *Create Coach Checkr*.  
```csharp
private static readonly CheckrOf<Case> CreateCourse =
    from name in Checkr.Input("Course Name",
        from cnt in Fuzzr.Counter("course-name") select $"Course-{cnt}")
    from trace in Checkr.Trace("Course Name", () => name)
    from course in Checkr.Act("Create Course", () => new Course(name))
    from checkName in Checkr.Spec("Course Name Set", () => course.Name == name)
    from toPool in Trackr.Pool("Store Course", course)
    select Case.Closed;
```
**Update Course Skills Checkr:**  
And again, same pattern as the *Update Coach Skills Checkr*.  
```csharp
private static readonly CheckrOf<Case> UpdateCourseSkills =
    from course in Trackr.OneOfPool<Course>("Course to update")
    from skills in Checkr.Input("Course Skills", Fuzzr.OneOf(SkillPool).Unique(Guid.NewGuid()).Many(3, 5))
    from courseNotNull in Checkr.When(() => course is not null,
        from traceCourse in Checkr.Trace("Course Name", () => course.Name)
        from updateSkills in Checkr.Act("Update Course Skills",
            () => course.UpdateRequiredSkills(skills))
        from checkSkills in Checkr.Spec("Course Required Skills Set",
            () => course.RequiredSkills.Count == skills.Count() && course.RequiredSkills.All(skills.Contains))
        select Case.Closed)
    select Case.Closed;
```
**Assigned Coaches Must Be Suitable Checkr:**  
And finally here's the system invariant. Any coach assigned a course must have the required skills to teach that course.  
```csharp
private static readonly CheckrOf<Case> AssignedCoachesMustBeSuitable =
    from coaches in Trackr.Pool<Coach>()
    from assignedCheck in Checkr.SpecIf("Assigned Coaches Must Be Suitable",
        () => coaches.Any(coach => coach.AssignedCourses.Any()),
        () => coaches.All(
            coach => coach.AssignedCourses.All(course => coach.IsSuitableFor(course))))
    select Case.Closed;
```
**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Notes\T_Examples\E_HorsesAgain\BoltingHorses.cs:53:1
 Original failing run:    20 executions
 Minimal failing case:    4 executions (after 16 shrinks)
 Seed:                    13918324
 ------------------------------------------------------------
  Executed (8): Create Coach
   - Trace: Coach Name = Coach-5
 ------------------------------------------------------------
  Executed (10): Create Course
   - Trace: Course Name = Course-3
 ------------------------------------------------------------
  Executed (13): Assign Coach to Course
   - Trace: Coach Name = Coach-5
   - Trace: Course Name = Course-3
 ------------------------------------------------------------
  Executed (20): Update Course Skills
   - Input: Course Skills = [ _ ]
   - Trace: Course Name = Course-3
 =====================================================
  !! Spec Failed: Assigned Coaches Must Be Suitable
 =====================================================
 Passed Specs
 - Course Name Set: 4x
 - Coach Name is set: 9x
 - Coach Skills Set: 2x
 - Coach is assigned: 4x
 - Assigned Coaches Must Be Suitable: 9x
 - Course Required Skills Set: 1x
 ------------------------------------------------------------
```
## It Is Written, ... But Only Once
In QuickCheckr, the strings you pass to `Trackr.Stashed`, `Checkr.Input`, `Checkr.Act`, etcetera,
are not just pretty captions in a report.

They function as **stable identifiers**.

QuickCheckr uses each key for two distinct purposes:

1. **Reporting:** The key appears in execution traces, minimal fail cases, and summaries.
2. **Book-keeping:** Internally, the engine uses the same key to label stored values, look up decisions, and coordinate shrinking.

Because of this dual role, **each key may only be written once per relevant scope**.  

If you try to write the same key in the same scope twice or more, using the same Checkr, an exception is thrown.
  
- `Trackr.Stashed`:  
```csharp
    from one in Trackr.Stashed(() => 42)
    from two in Trackr.Stashed(() => 666)
    from spec in Checkr.Spec("Always passes", () => true)
    select Case.Closed;
```
  Throws:  
```text
Key 'System.Int32' is written more than once in a single 'Trackr.Stashed' run.
QuickCheckr treats keys as stable identifiers. 
Reusing a key for multiple write operations makes reports and shrinking ambiguous.
Possible solutions:
- Ensure each 'Trackr.Stashed' uses a unique key.
```
- `Checkr.Input`:  
```csharp
    from one in Checkr.Input("reused", Fuzzr.Int())
    from two in Checkr.Input("reused", Fuzzr.Int())
    from spec in Checkr.Spec("Always passes", () => true)
    select Case.Closed;
```
  Throws:  
```text
Key 'reused' is written more than once in a single 'Checkr.Input' execution.
QuickCheckr treats keys as stable identifiers. 
Reusing a key for multiple write operations makes reports and shrinking ambiguous.
Possible solutions:
- Ensure each 'Checkr.Input' uses a unique key.
```
## Acting Out Of Sync
```csharp
public class Eventually
{
    public static Task<int> AddOne(int input)
    {
        return Task.FromResult(input + 1);
    }
}
```
```csharp
    from input in Checkr.Input("Input", Fuzzr.Int())
    from result in Checkr.Act("Eventually.AddOne", () => Eventually.AddOne(input))
    from spec in Checkr.Spec("Is not 42", () => result != 42)
    select Case.Closed;
```
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Notes\Y_Async\ActingOutOfSync.cs:20:1
 Original failing run:    1 execution
 Seed:                    367926403
 ------------------------------------------------------------
  Executed (1): Eventually.AddOne
   - Input: Input = 41
 =============================
  !! Spec Failed: Is not 42
 =============================
```
