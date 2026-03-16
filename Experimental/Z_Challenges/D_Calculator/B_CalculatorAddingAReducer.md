# Calculator Adding A Reducer
And a DeliberationPolicy.  
The result presented here is ok, but in reality, this sometimes produces non-explanatory results.  

**The Checkr:**  
```csharp
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
        caseFile.InputsNamed<Expr>("Calculator", a => DepthOf(a)),
    DeliberationTarget = 1,
    ReportMode = config.ReportMode & ~ReportMode.StackTrace
};
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                B_CalculatorAddingAReducer.cs:58:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 10 shrinks)
 Seed:                    1301314441
 ------------------------------------------------------------
  Executed: Evaluate
   - Input: Calculator = { Op: Div, R: { Op: Div, L: { L: { Value: 3 }, R: { Value: 2 } }, R: { Value: 7 } } }
            Redux        { Op: Div, R: { Op: Div, L: { L: { Value: 1 }, R: { Value: 1 } }, R: { Value: 3 } } }
   - Trace: Original   = { Op: Div, L: { Op: Add, L: { Value: 6 }, R: { Value: 7 } }, R: { Op: Div, L: { Op: Add, L: { Value: 3 }, R: { Value: 2 } }, R: { Value: 7 } } }
 ===========================================================================
  !! DivideByZeroException: Attempted to divide by zero.
 ===========================================================================
```
