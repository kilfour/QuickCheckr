using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Experimental.Z_Challenges.D_Calculator;

public static class CalculatorFuzzr
{
    [CodeSnippet]
    public static readonly FuzzrOf<Expr> TheFuzzr =
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
        select expr;
}