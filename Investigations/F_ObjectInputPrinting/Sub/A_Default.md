# Default

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
   at QuickCheckr.Tests.Drafts.J_YouCanGoYourOwnWay.N_Showr.Reference.Term.Evaluate() in C:\Code\QuickCheckr\QuickCheckr.Tests\Investigations\F_ObjectInputPrinting\ObjectInputPrinting.cs:line 33
   at QuickCheckr.Checkr.<>c__DisplayClass10_1`1.<TryCatch>b__1() in C:\Code\QuickCheckr\QuickCheckr\CheckrAct_Helper.cs:line 13
   at QuickCheckr.UnderTheHood.TheCatcher.InTheCheckr[T](CheckrState state, String key, Func`1 func, Action`1 onException) in C:\Code\QuickCheckr\QuickCheckr\UnderTheHood\TheCatcher.cs:line 15
 ===========================================================================
```
