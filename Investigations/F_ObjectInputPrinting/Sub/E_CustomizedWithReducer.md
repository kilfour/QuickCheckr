# Customized With Reducer

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
        R = new Term { L = new Num { Value = 10 }, R = new Num { Value = 20 }, Op = Operation.Div },
        Op = Operation.Div
    }),
    Shrink.OnType<int>(s => s.Simplify(Reduce.Towards(1))))
from trace in Checkr.Trace("Original", () => expr)
from eval in Checkr.Act("Evaluate", expr.Evaluate)
from expect in Checkr.Expect("Not 42", () => eval != 42)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                E_CustomizedWithReducer.cs:32:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 8 shrinks)
 Seed:                    1611568796
 ------------------------------------------------------------
  Executed: Evaluate
   - Input: Expr     = ( _ / ( 10 / 20 ) )
            Redux      ( _ / ( 1 / 2 ) )
   - Trace: Original = { Op: Div, L: { Value: 84 }, R: { Op: Div, L: { Value: 10 }, R: { Value: 20 } } }
 ===========================================================================
  !! System.DivideByZeroException: Attempted to divide by zero.
   at QuickCheckr.Tests.Drafts.J_YouCanGoYourOwnWay.N_Showr.Reference.Term.Evaluate() in C:\Code\QuickCheckr\QuickCheckr.Tests\Investigations\F_ObjectInputPrinting\ObjectInputPrinting.cs:line 33
   at QuickCheckr.Checkr.<>c__DisplayClass10_1`1.<TryCatch>b__1() in C:\Code\QuickCheckr\QuickCheckr\CheckrAct_Helper.cs:line 13
   at QuickCheckr.UnderTheHood.TheCatcher.InTheCheckr[T](CheckrState state, String key, Func`1 func, Action`1 onException) in C:\Code\QuickCheckr\QuickCheckr\UnderTheHood\TheCatcher.cs:line 15
 ===========================================================================
 Passed Expectations
 - Not 42: 1x
 ------------------------------------------------------------
```
