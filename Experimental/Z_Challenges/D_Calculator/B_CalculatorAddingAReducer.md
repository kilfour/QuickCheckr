# Calculator Adding A Reducer
And a DeliberationPolicy.  
The result presented here is ok, but in reality, this sometimes produces non-explanatory results.  

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
    Shrink.OnType<int>(s => s.Simplify(Reduce.Towards(1))))
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
 Location:                B_CalculatorAddingAReducer.cs:58:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 4 shrinks)
 Seed:                    48165483
 ------------------------------------------------------------
  Executed: Evaluate
   - Input: Calculator = ( _ / ( 0 / 9 ) )
            Redux        { Op: /, R: { Op: /, L: { Value: 1 }, R: { Value: 2 } } }
   - Trace: Original   = { Op: Div, L: { Value: 5 }, R: { Op: Div, L: { Value: 0 }, R: { Value: 9 } } }
 ===========================================================================
  !! DivideByZeroException: Attempted to divide by zero.
 ===========================================================================
```
