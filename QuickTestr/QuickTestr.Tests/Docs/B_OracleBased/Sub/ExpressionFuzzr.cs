using QuickCheckr;
using QuickFuzzr;
using QuickPulse.Explains;


namespace QuickTestr.Tests.Docs.B_OracleBased.Sub;

[CodeExample]
public class ExpressionFuzzr
{
    public static FuzzrOf<string> GetExpression(int maxDepth = 4) => Expr(maxDepth);

    public static readonly FuzzrOf<string> Number = from i in Fuzzr.Int(1, 4) select i.ToString();
    private static readonly FuzzrOf<string> AddOp = Fuzzr.OneOf("+", "-");
    private static readonly FuzzrOf<string> MulOp = Fuzzr.OneOf("*", "/");
    private static readonly FuzzrOf<string> PowOp = Fuzzr.Constant("^");

    private static readonly FuzzrOf<string> WS =
        from chs in Fuzzr.Constant(' ').Many(1)
        select new string([.. chs]);

    private static readonly FuzzrOf<string> UnaryOp = Fuzzr.OneOf("-");

    private static FuzzrOf<string> Primary(int maxDepth) =>
        maxDepth <= 0
        ? Number
        : Fuzzr.OneOf(
            Number,
            from l in Fuzzr.Constant("(")
            from e in Expr(maxDepth - 1)
            from r in Fuzzr.Constant(")")
            select l + e + r,
            from op in UnaryOp
            from ws in WS
            from inner in Primary(maxDepth - 1)
            select op + ws + inner
          );

    private static FuzzrOf<string> Factor(int maxDepth) =>
        from a in Primary(maxDepth)
        from tail in maxDepth <= 0
            ? Fuzzr.Constant("")
            : Fuzzr.OneOf(
                  Fuzzr.Constant(""),
                  from w1 in WS
                  from op in PowOp
                  from w2 in WS
                  from b in Factor(maxDepth - 1)
                  select w1 + op + w2 + b)
        select a + tail;

    private static FuzzrOf<string> Term(int maxDepth) =>
        from head in Factor(maxDepth)
        from pieces in (
            from w1 in WS
            from op in MulOp
            from w2 in WS
            from rhs in Factor(maxDepth - 1)
            select w1 + op + w2 + rhs
        ).Many(0, 3)
        select head + string.Concat(pieces);

    private static FuzzrOf<string> Expr(int maxDepth) =>
        from depth in Fuzzr.Int(0, maxDepth)
        from head in Term(depth)
        from pieces in (
            from w1 in WS
            from op in AddOp
            from w2 in WS
            from rhs in Term(depth - 1)
            select w1 + op + w2 + rhs
        ).Many(0, 3)
        select head + string.Concat(pieces);
}







