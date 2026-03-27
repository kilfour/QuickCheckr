# Calculator Adding A Reducer
And a DeliberationPolicy.  

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
from input in Checkr.Input("Calculator", TheFuzzr,
    Shrink.When<int>(s => s
            .As<Num>(a => a.Value)
            .As<Term, Expr>(a => a.R)
            .Value(t => t.Op == Operation.Div),
        s => s.Simplify(Reduce.Towards(1))),
    Shrink.OnType<int>(s => s.Simplify(Reduce.Towards(0))))
from trace in Checkr.Trace("Original", () => input)
from eval in Checkr.Act("Evaluate", input.Evaluate)
select Case.Closed;
```

**The Configuration:**  
```csharp
config with
{
    DeliberationPolicy = caseFile =>
        (caseFile.InputCount == 0 ? 100 : 0) +
        caseFile.InputsNamed<Expr>("Calculator", a => DepthOf(a)),
    DeliberationTarget = 2,
    ReportMode = config.ReportMode & ~ReportMode.StackTrace
};
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                B_CalculatorAddingAReducer.cs:32:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 12 shrinks)
 Seed:                    376086434
 ------------------------------------------------------------
  Executed: Evaluate
   - Input: Calculator = ( _ / ( 8 / _ ) )
            Redux        ( _ / ( 0 / _ ) )
   - Trace: Original   = { Op: Div, L: { Op: Add, L: { Value: 4 }, R: { Value: 6 } }, R: { Op: Div, L: { Value: 8 }, R: { Value: 9 } } }
 ===========================================================================
  !! DivideByZeroException: Attempted to divide by zero.
 ===========================================================================
```
