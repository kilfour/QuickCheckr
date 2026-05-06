# Unchecked Expectations

**The Checkr:**  
```csharp
from input in Checkr.Input("input", Fuzzr.Counter("count"))
from expect in Checkr.ExpectWhen("checked", () => input > 1, () => false)
from expectUnchecked in Checkr.ExpectWhen("unchecked", () => input > 2, () => false)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                UncheckedExpectationsTest.cs:40:1
 Original failing run:    2 executions
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    96544855
 ------------------------------------------------------------
  Executed:
   - Input: input = 2
 ==================================
  !! Expectation Failed: checked
 ==================================
 ! Warning: Unchecked Expectations !
 - unchecked
 ------------------------------------------------------------
```
### Expectation Throws
In case an expectation throws an exception during the initial Checkr run, 
It is wrapped in an ExpectationThrowsException through InnerException and the expectation key is reported.  

**The Checkr:**  
```csharp
from input in Checkr.Input("Input", Fuzzr.Int())
from act in Checkr.Act("Act", () => (int?)null)
from expect in Checkr.Expect("Expect Throws", () => act.Value == 5)
select Case.Closed;
```

**Message::**  
```csharp
"The expectation 'Expect Throws' threw a 'InvalidOperationException'.";
```
### Caused By Delayed Result

**The Checkr:**  
```csharp
from input in Checkr.Input("Input", Fuzzr.Int())
from act in Checkr.ActCarefully("Act", () => (int?)null)
from expect in Checkr.Expect("Expect Throws", () => act.Value!.Value == 5)
select Case.Closed;
```

**Message::**  
```csharp
"The expectation 'Expect Throws' threw a 'DelayedResultNullValueAccessedException'.";
```
## Reports Redux

**The Checkr:**  
```csharp
from input in Checkr.Input("Input", Fuzzr.Int(), Shrink.OnType<int>(s => s.Simplify(Reduce.TowardsZero)))
from trace in Checkr.Trace("Input", () => input)
from expect in Checkr.Expect("<= 42", () => input <= 42)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                A_ReportsRedux.cs:32:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    1883509239
 ------------------------------------------------------------
  Executed:
   - Input: Input = 69
            Redux   43
   - Trace: Input = 69
   - Final: Input = 43
 ================================
  !! Expectation Failed: <= 42
 ================================
```
## Checkr Sequence
Fixed this one by using Testimonys, see Memory.  

**The Checkr:**  
```csharp
from counter in Checkr.Input("Counter", Fuzzr.Counter("counter"))
from sequence in Checkr.Sequence(
    Checkr.Act("1", () => { if (counter == 3) ComputerSays.No("BOOM"); }),
    Checkr.Act("2", () => { })
)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                CheckrSequence.cs:41:1
 Original failing run:    3 executions
 Minimal failing case:    1 execution (after 2 shrinks)
 Seed:                    207626985
 ------------------------------------------------------------
  Executed: 1
   - Input: Counter = 3
 ===========================================================================
  !! QuickPulse.Instruments.ComputerSaysNo: BOOM
   at QuickPulse.Instruments.ComputerSays.No(String msg)
   at QuickCheckr.Tests.Investigations.Sequence.CheckrSequence.<>c__DisplayClass7_0.<TheCheckr>b__2() in C:\Code\QuickCheckr\QuickCheckr.Tests\Investigations\Sequence\CheckrSequence.cs:line 30
   at QuickCheckr.Checkr.<>c__DisplayClass2_0.<Act>b__0() in C:\Code\QuickCheckr\QuickCheckr\CheckrAct.cs:line 24
   at QuickCheckr.Checkr.<>c__DisplayClass10_1`1.<TryCatch>b__1() in C:\Code\QuickCheckr\QuickCheckr\CheckrAct_Helper.cs:line 15
   at QuickCheckr.UnderTheHood.TheCatcher.InTheCheckr[T](CheckrState state, Func`1 func, Action`1 onException) in C:\Code\QuickCheckr\QuickCheckr\UnderTheHood\TheCatcher.cs:line 12
 ===========================================================================
```
## Greed Is Not Good
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
