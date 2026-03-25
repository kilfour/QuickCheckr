# Calculator

The challenge involves a simple calculator language representing expressions consisting of integers,
their additions and divisions only, like 1 + (2 / 3).

The property being tested is that

- if we have no subterms of the form x / 0,  
- then we can evaluate the expression without a zero division error.
This property is false, because we might have a term like 1 / (3 + -3), in which the divisor is not literally 0 but evaluates to 0.

One of the possible difficulties that might come up is the shrinking of recursive expressions.
  

**The Testr:**  
```csharp
Testr
    .Named("No division by zero")
    .For(TheFuzzr, TheShrinkr)
    .Format(TheFormatr)
    .Deliberate(a => DepthOf(a))
    .Assert(a => { a.Evaluate(); return true; });
```

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

**The Shrinkr:**  
```csharp
Shrink.When<int>(s => s
    .As<Num>(a => a.Value)
    .As<Term, Expr>(a => a.R)
    .Value(t => t.Op == Operation.Div),
s => s.Simplify(Reduce.Towards(1)))
```

**The Formatter:**  
```csharp
[ Showr.ForClassification(a => a.For<Term>()
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
, Showr.ForClassification(a => a.For<Num>()
    .Bounds("", "")
    .Delimiter("")
    .HidePropertyNames())]
```

**The Helper:**  
```csharp
private static int DepthOf(Expr a, int depth = 0)
{
    if (a is Term term)
        return Math.Max(DepthOf(term.L, depth + 1), DepthOf(term.R, depth + 1));
    return depth;
}
```

**The Report:**  
```text
------------------------------------------------------------
  DivideByZeroException: Attempted to divide by zero.
  Seed: 729093046
 ------------------------------------------------------------
  Falsified:
    Input = ( _ / ( 2 / _ ) )
    Redux = ( _ / ( 0 / _ ) )

  Original:
    { Op: Div, L: { Value: 4 }, R: { Op: Div, L: { Value: 2 }, R: { Value: 3 } } }
 ------------------------------------------------------------
```
