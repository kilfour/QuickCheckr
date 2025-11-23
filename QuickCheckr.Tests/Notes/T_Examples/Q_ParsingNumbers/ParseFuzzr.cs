using QuickCheckr;
using QuickFuzzr;


namespace QuickCheckr.Tests.Notes.T_Examples.Q_ParsingNumbers;

public class ParseFuzzr
{
    public record FuzzrResult(string Text, double Value)
    {
        public static FuzzrResult Empty => new("", 0);
        public static FuzzrResult operator +(FuzzrResult a, FuzzrResult b)
            => new(a.Text + b.Text, a.Value + b.Value);
    }

    // Number: both text and numeric value
    private static readonly FuzzrOf<FuzzrResult> Number =
        from i in Fuzzr.Int(1, 4)
        select new FuzzrResult(i.ToString(), i);

    // Operators: text = operator, value = 0 (neutral)
    private static readonly FuzzrOf<FuzzrResult> AddOp =
        from op in Fuzzr.OneOf("+", "-")
        select new FuzzrResult(op, 0);

    private static readonly FuzzrOf<FuzzrResult> MulOp =
        from op in Fuzzr.OneOf("*", "/")
        select new FuzzrResult(op, 0);

    private static readonly FuzzrOf<FuzzrResult> PowOp =
        from op in Fuzzr.Constant("^")
        select new FuzzrResult(op, 0);

    // Whitespace
    private static readonly FuzzrOf<FuzzrResult> WS =
        from chs in Fuzzr.Constant(' ').Many(0, 3)
        select new FuzzrResult(new string([.. chs]), 0);

    private static readonly FuzzrOf<FuzzrResult> UnaryOp =
        from op in Fuzzr.OneOf("-")
        select new FuzzrResult(op, 0);

    private static FuzzrOf<FuzzrResult> Primary(int maxDepth) =>
    maxDepth <= 0
        ? Number
        : Fuzzr.OneOf(
            // Number
            Number,

            // Parenthesized expression
            from l in Fuzzr.Constant(new FuzzrResult("(", 0))
            from e in Expr(maxDepth - 1)
            from r in Fuzzr.Constant(new FuzzrResult(")", 0))
            select l + e + r
        );

    private static FuzzrOf<FuzzrResult> Power(int maxDepth) =>
    from a in Primary(maxDepth)
    from tail in (
        maxDepth <= 0
            ? Fuzzr.Constant(FuzzrResult.Empty)
            : Fuzzr.OneOf(
                Fuzzr.Constant(FuzzrResult.Empty),
                from w1 in WS
                from op in PowOp
                from w2 in WS
                from b in Power(maxDepth - 1)    // right-assoc: Power → "^" Power
                select new FuzzrResult(
                    w1.Text + op.Text + w2.Text + b.Text,
                    (int)Math.Pow(a.Value, b.Value)
                )
            )
    )
    select tail == FuzzrResult.Empty
        ? a
        : new FuzzrResult(a.Text + tail.Text, tail.Value);  // don't add a.Value again

    private static FuzzrOf<FuzzrResult> Unary(int maxDepth) =>
    maxDepth <= 0
        ? Power(0)
        : Fuzzr.OneOf(

            // unary negation consumes depth
            from op in UnaryOp
            from ws in WS
            from inner in Unary(maxDepth - 1)
            select new FuzzrResult(
                op.Text + ws.Text + inner.Text,
                -inner.Value
            ),

            // no unary, just power
            Power(maxDepth)
        );


    // Factor ::= Primary ^ Factor?
    private static FuzzrOf<FuzzrResult> Factor(int maxDepth) =>
        from head in Unary(maxDepth)
        from pieces in (
            from w1 in WS
            from op in MulOp
            from w2 in WS
            from rhs in Unary(maxDepth - 1)
            select new { w1, op, w2, rhs }
        ).Many(0, 3)
        select pieces.Aggregate(
            head,
            (acc, p) =>
            {
                var opText = p.w1.Text + p.op.Text + p.w2.Text + p.rhs.Text;
                var newText = acc.Text + opText;
                var newValue = p.op.Text == "*"
                    ? acc.Value * p.rhs.Value
                    : acc.Value / p.rhs.Value;   // int division as before
                return new FuzzrResult(newText, newValue);
            });


    // Term ::= Factor ( (*|/) Factor )*
    private static FuzzrOf<FuzzrResult> Term(int maxDepth) =>
        from head in Factor(maxDepth)
        from pieces in (
            from w1 in WS
            from op in MulOp
            from w2 in WS
            from rhs in Factor(maxDepth - 1)
            select new { w1, op, w2, rhs }
        ).Many(0, 3)
        select pieces.Aggregate(
            head,
            (acc, p) =>
            {
                var opText = p.w1.Text + p.op.Text + p.w2.Text + p.rhs.Text;
                var newText = acc.Text + opText;
                var newValue = p.op.Text == "*"
                    ? acc.Value * p.rhs.Value
                    : acc.Value / p.rhs.Value;
                return new FuzzrResult(newText, newValue);
            });

    // Expr ::= Term ( (+|-) Term )*
    private static FuzzrOf<FuzzrResult> Expr(int maxDepth) =>
        from depth in Fuzzr.Int(0, maxDepth)
        from head in Term(depth)
        from pieces in (
            from w1 in WS
            from op in AddOp
            from w2 in WS
            from rhs in Term(depth - 1)
            select new { w1, op, w2, rhs }
        ).Many(0, 3)
        select pieces.Aggregate(
            head,
            (acc, p) =>
            {
                var opText = p.w1.Text + p.op.Text + p.w2.Text + p.rhs.Text;
                var newText = acc.Text + opText;
                var newValue = p.op.Text == "+"
                    ? acc.Value + p.rhs.Value
                    : acc.Value - p.rhs.Value;
                return new FuzzrResult(newText, newValue);
            });

    public static FuzzrOf<FuzzrResult> Expression(int maxDepth = 4)
        => Expr(maxDepth);
}







