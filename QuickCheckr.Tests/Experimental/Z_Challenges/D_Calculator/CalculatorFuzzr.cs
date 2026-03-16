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
                    if (t.Operation == Operation.Divide && t.Right is Scalar { Value: 0 })
                        t.Right = new Scalar { Value = 1 };
            })
        from depth in Configr<Expr>.Depth(3, 10)
        from inheritance in Configr<Expr>.AsOneOf(typeof(Term), typeof(Scalar))
        from terminator in Configr<Expr>.EndOn<Scalar>()
        from expr in Fuzzr.One<Expr>()
        select expr;
}