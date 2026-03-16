# Calculator

The challenge involves a simple calculator language representing expressions consisting of integers,
their additions and divisions only, like 1 + (2 / 3).

The property being tested is that

- if we have no subterms of the form x / 0,  
- then we can evaluate the expression without a zero division error.
This property is false, because we might have a term like 1 / (3 + -3), in which the divisor is not literally 0 but evaluates to 0.

One of the possible difficulties that might come up is the shrinking of recursive expressions.
  

**The Fuzzr:**  
```csharp
from i in Configr.Primitive(Fuzzr.Int(0, 10))
from noZeroDiv in Configr<Expr>.Apply(
    a =>
    {
        // rewrite division by zero terms
        if (a is Term t)
            if (t.Op == Operation.Div && t.R is Num { Value: 0 })
                t.R = new Num { Value = 1 };
    })
from depth in Configr<Expr>.Depth(0, 10)
from inheritance in Configr<Expr>.AsOneOf(typeof(Term), typeof(Num))
from terminator in Configr<Expr>.EndOn<Num>()
from expr in Fuzzr.One<Expr>()
select expr
```

**The Checkr:**  
```csharp
from input in Checkr.Input("Calculator", TheFuzzr)
from trace in Checkr.Trace("Original", () => input)
from eval in Checkr.Act("Evaluate", input.Evaluate)
select Case.Closed;
```

**The Configuration:**  
```csharp
config with { ReportMode = config.ReportMode & ~ReportMode.StackTrace };
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                A_Calculator.cs:58:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 2 shrinks)
 Seed:                    1264796639
 ------------------------------------------------------------
  Executed: Evaluate
   - Input: Calculator = { Op: Div, R: { Op: Div, L: { Value: 1 }, R: { Value: 7 } } }
   - Trace: Original   = { Op: Div, L: { Value: 4 }, R: { Op: Div, L: { Value: 1 }, R: { Value: 7 } } }
 ===========================================================================
  !! DivideByZeroException: Attempted to divide by zero.
 ===========================================================================
```
