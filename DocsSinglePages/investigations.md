## Expectation Exceptions
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
## Object Input Printing
### Default

**The Checkr:**  
```csharp
from expr in Checkr.Input("Expr", Fuzzr.Constant(
    new Term { L = new Num { Value = 42 }, R = new Num { Value = 0 }, Op = Operation.Div }))
from trace in Checkr.Trace("Original", () => expr)
from eval in Checkr.Act("Evaluate", expr.Evaluate)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                A_Default.cs:58:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 2 shrinks)
 Seed:                    844205832
 ------------------------------------------------------------
  Executed: Evaluate
   - Input: Expr     = { Op: Div, R: { Value: 0 } }
   - Trace: Original = { Op: Div, L: { Value: 42 }, R: { Value: 0 } }
 ===========================================================================
  !! System.DivideByZeroException: Attempted to divide by zero.
   at QuickCheckr.Tests.Investigations.F_ObjectInputPrinting.Term.Evaluate() in C:\Code\QuickCheckr\QuickCheckr.Tests\Investigations\F_ObjectInputPrinting\ObjectInputPrinting.cs:line 33
   at QuickCheckr.Checkr.<>c__DisplayClass10_1`1.<TryCatch>b__1() in C:\Code\QuickCheckr\QuickCheckr\CheckrAct_Helper.cs:line 13
   at QuickCheckr.UnderTheHood.TheCatcher.InTheCheckr[T](CheckrState state, String key, Func`1 func, Action`1 onException) in C:\Code\QuickCheckr\QuickCheckr\UnderTheHood\TheCatcher.cs:line 15
 ===========================================================================
```
### Customized

**The Checkr:**  
```csharp
from showrTerm in Showr.ForClassification(a => a.For<Term>()
    .Bounds("( ", " )")
    .Delimiter(" ")
    .HidePropertyNames()
    .Property(a => a.L)
    .Property(a => a.Op, a =>
        a switch
        {
            Operation.Add => "+",
            Operation.Div => "/",
            _ => throw new ArgumentOutOfRangeException()
        })
    .Property(a => a.R))
from showrOp in Showr.ForClassification(a => a.For<Num>()
    .Bounds("", "")
    .Delimiter("")
    .HidePropertyNames())
from expr in Checkr.Input("Expr", Fuzzr.Constant(
    new Term { L = new Num { Value = 84 }, R = new Num { Value = 2 }, Op = Operation.Div }))
from trace in Checkr.Trace("Original", () => expr)
from eval in Checkr.Act("Evaluate", expr.Evaluate)
from expect in Checkr.Expect("Not 42", () => eval != 42)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                B_Customized.cs:32:1
 Original failing run:    1 execution
 Seed:                    1611568796
 ------------------------------------------------------------
  Executed: Evaluate
   - Input: Expr     = ( 84 / 2 )
   - Trace: Original = { Op: Div, L: { Value: 84 }, R: { Value: 2 } }
 =================================
  !! Expectation Failed: Not 42
 =================================
```
### Customized Nested

**The Checkr:**  
```csharp
from showrTerm in Showr.ForClassification(a => a.For<Term>()
    .Bounds("( ", " )")
    .Delimiter(" ")
    .HidePropertyNames()
    .Property(a => a.L)
    .Property(a => a.Op, a =>
        a switch
        {
            Operation.Add => "+",
            Operation.Div => "/",
            _ => throw new ArgumentOutOfRangeException()
        })
    .Property(a => a.R))
from showrOp in Showr.ForClassification(a => a.For<Num>()
    .Bounds("", "")
    .Delimiter("")
    .HidePropertyNames())
from expr in Checkr.Input("Expr", Fuzzr.Constant(
    new Term
    {
        L = new Num { Value = 84 },
        R = new Term { L = new Num { Value = 1 }, R = new Num { Value = 1 }, Op = Operation.Add },
        Op = Operation.Div
    }))
from trace in Checkr.Trace("Original", () => expr)
from eval in Checkr.Act("Evaluate", expr.Evaluate)
from expect in Checkr.Expect("Not 42", () => eval != 42)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                C_CustomizedNested.cs:32:1
 Original failing run:    1 execution
 Seed:                    1611568796
 ------------------------------------------------------------
  Executed: Evaluate
   - Input: Expr     = ( 84 / ( 1 + 1 ) )
   - Trace: Original = { Op: Div, L: { Value: 84 }, R: { Op: Add, L: { Value: 1 }, R: { Value: 1 } } }
 =================================
  !! Expectation Failed: Not 42
 =================================
```
### Customized With Irrelevant

**The Checkr:**  
```csharp
from showrTerm in Showr.ForClassification(a => a.For<Term>()
    .Bounds("( ", " )")
    .Delimiter(" ")
    .HidePropertyNames()
    .Property(a => a.L)
    .Property(a => a.Op, a =>
        a switch
        {
            Operation.Add => "+",
            Operation.Div => "/",
            _ => throw new ArgumentOutOfRangeException()
        })
    .Property(a => a.R))
from showrOp in Showr.ForClassification(a => a.For<Num>()
    .Bounds("", "")
    .Delimiter("")
    .HidePropertyNames())
from expr in Checkr.Input("Expr", Fuzzr.Constant(
    new Term { L = new Num { Value = 84 }, R = new Num { Value = 0 }, Op = Operation.Div }))
from trace in Checkr.Trace("Original", () => expr)
from eval in Checkr.Act("Evaluate", expr.Evaluate)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                D_CustomizedWithIrrelevant.cs:32:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 2 shrinks)
 Seed:                    1611568796
 ------------------------------------------------------------
  Executed: Evaluate
   - Input: Expr     = ( _ / 0 )
   - Trace: Original = { Op: Div, L: { Value: 84 }, R: { Value: 0 } }
 ===========================================================================
  !! System.DivideByZeroException: Attempted to divide by zero.
   at QuickCheckr.Tests.Investigations.F_ObjectInputPrinting.Term.Evaluate() in C:\Code\QuickCheckr\QuickCheckr.Tests\Investigations\F_ObjectInputPrinting\ObjectInputPrinting.cs:line 33
   at QuickCheckr.Checkr.<>c__DisplayClass10_1`1.<TryCatch>b__1() in C:\Code\QuickCheckr\QuickCheckr\CheckrAct_Helper.cs:line 13
   at QuickCheckr.UnderTheHood.TheCatcher.InTheCheckr[T](CheckrState state, String key, Func`1 func, Action`1 onException) in C:\Code\QuickCheckr\QuickCheckr\UnderTheHood\TheCatcher.cs:line 15
 ===========================================================================
```
## Greed Is Not Good

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
## Nested Collections Inner Count
Not necessarily a bug, but not a helpful report either. 

There is a real bug in the shrink count though.  
Only the outer list shrink (being deemed irrelevant) is counted, minor issue.
  

**The Checkr:**  
```csharp
from input in Checkr.Input("List", Fuzzr.Int().Many(1, 5).Many(3, 10))
from trace in Checkr.Trace("Original", () => input)
from expect in Checkr.Expect("List count >= 4", () => input.Count() >= 4)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                B_NestedCollectionsInnerCount.cs:32:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    1427464097
 ------------------------------------------------------------
  Executed:
   - Trace: Original = [ [ 41, 73, 3, 5, 14 ], [ 2, 88, 10 ], [ 59, 61, 21, 32, 34 ] ]
   - WARNING: All inputs were considered irrelevant.
   - WARNING: Empty collection deemed irrelevant at input 'List'.
 ==========================================
  !! Expectation Failed: List count >= 4
 ==========================================
```
## Nested Collections Inner Count
Adding the custom shrinker solves the problem in the report,
but really coming up with that solution is not something you want the user to do.
  

**The Checkr:**  
```csharp
from input in Checkr.Input("List", Fuzzr.Int().Many(1, 5).Many(3, 10),
    Shrink.CollectionFromState<IEnumerable<int>>([[], [], [], [], []], []))
from trace in Checkr.Trace("Original", () => input)
from expect in Checkr.Expect("List count >= 4", () => input.Count() >= 4)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                B1_NestedCollectionsInnerCount.cs:32:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    1134566081
 ------------------------------------------------------------
  Executed:
   - Input: List     = [ [ 79, 83, 60, 28, 36 ], [ 92, 99 ], [ 91 ] ]
            Redux      [ ]
   - Trace: Original = [ [ 79, 83, 60, 28, 36 ], [ 92, 99 ], [ 91 ] ]
 ==========================================
  !! Expectation Failed: List count >= 4
 ==========================================
```
