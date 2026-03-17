# Customized Nested

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
