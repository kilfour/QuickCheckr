using QuickCheckr;
using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Experimental.Z_Challenges.D_Calculator
{
    [DocFile]
    [DocContent(
    @"
The challenge involves a simple calculator language representing expressions consisting of integers,
their additions and divisions only, like 1 + (2 / 3).

The property being tested is that

- if we have no subterms of the form x / 0,  
- then we can evaluate the expression without a zero division error.
This property is false, because we might have a term like 1 / (3 + -3), in which the divisor is not literally 0 but evaluates to 0.

One of the possible difficulties that might come up is the shrinking of recursive expressions.
")]
    public class Calculator : CheckrTest<Calculator>
    {
        protected override bool Asserts => false;
        protected override bool Report => true;
        protected override bool Explain => true;

        [Fact(Skip = "wip")]
        [DocCheckrHeader]
        [DocCheckr]
        [DocReportHeader]
        [DocReport]
        public override void Example() =>
            Run(767678629);

        public enum Operation { Add, Divide }

        public abstract record Expr
        {
            public abstract int Evaluate();
        }

        public record Scalar : Expr
        {
            public int Value { get; set; } = 0;
            public override int Evaluate() => Value;
            public override string ToString() => Value.ToString();
        }

        public record Term : Expr
        {
            public Operation Operation { get; set; } = Operation.Add;
            public Expr Left { get; set; } = new Scalar();
            public Expr Right { get; set; } = new Scalar();

            public override int Evaluate() =>
                Operation switch
                {
                    Operation.Add => Left.Evaluate() + Right.Evaluate(),
                    Operation.Divide => Left.Evaluate() / Right.Evaluate(),
                    _ => 0
                };

            public override string ToString()
            {
                var left = Left.ToString();
                var right = Right.ToString();
                var operation = Operation switch
                {
                    Operation.Add => "+",
                    Operation.Divide => "/",
                    _ => ""
                };
                return $"({left} {operation} {right})";
            }
        }

        private static readonly FuzzrOf<Expr> TheFuzzr =
            from i in Configr.Primitive(Fuzzr.Int(0, 10))
            from noZeroDiv in Configr<Expr>.Apply(
                a =>
                {
                    if (a is Term t)
                        if (t.Operation == Operation.Divide && t.Right is Scalar { Value: 0 })
                            t.Right = new Scalar { Value = 1 };
                })
            from depth in Configr<Expr>.Depth(3, 10)
            from inheritance in Configr<Expr>.AsOneOf(typeof(Term), typeof(Scalar))
            from terminator in Configr<Expr>.EndOn<Scalar>()
            from expr in Fuzzr.One<Expr>()
            select expr;


        [CodeSnippet]
        protected override CheckrOf<Case> TheCheckr() =>
            from input in Checkr.Input("Calculator", TheFuzzr)
            from trace in Checkr.Trace("Original", () => input)
            from eval in Checkr.Act("Evaluate", () => input.Evaluate())
            select Case.Closed;

        protected override void Verify(Article article)
        {

        }
    }
}
