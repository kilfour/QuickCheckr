using QuickTestr.Tests.Tools;
using QuickFuzzr;
using QuickPulse.Explains;
using QuickCheckr;
using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Authoring.ThePress;

namespace QuickTestr.Tests.Challenges.D_Calculator;

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
public class Calculator : QuickTestrPropertyTest<Calculator>
{
    protected override bool Asserts => false;
    protected override bool Report => false;
    protected override bool Explain => false;

    [Fact]
    [DocTestrHeader]
    [DocTestr]
    [DocFuzzrHeader]
    [DocExample(typeof(Calculator), nameof(TheFuzzr))]
    [DocBoldHeader("The Shrinkr")]
    [DocExample(typeof(Calculator), nameof(TheShrinkr))]
    [DocBoldHeader("The Formatter")]
    [DocExample(typeof(Calculator), nameof(TheFormatr))]
    [DocBoldHeader("The Helper")]
    [DocExample(typeof(Calculator), nameof(DepthOf))]
    [DocReportHeader]
    [DocReport]
    public override void Example() => Document();

    [CodeSnippet]
    [CodeRemoveJournalist]
    [CodeRemove("12901993")]
    protected override void GetTestr(Journalist journalist) =>
        Testr.Named("No division by zero")
            .Format(TheFormatr)
            .For(TheFuzzr, TheShrinkr)
            .Deliberate(a => DepthOf(a))
            .Assert(a => { a.Evaluate(); return true; })
            .StoreCaseFiles(journalist)
            .Run(729093046);

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

    [CodeSnippet]
    private static readonly Shrinker TheShrinkr =
        Shrink.When<int>(s => s
            .As<Num>(a => a.Value)
            .As<Term, Expr>(a => a.R)
            .Value(t => t.Op == Operation.Div),
        s => s.Simplify(Reduce.Towards(1)));

    [CodeSnippet]
    private readonly CheckrOf<Case>[] TheFormatr =
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
            .HidePropertyNames())];

    [CodeExample]
    private static int DepthOf(Expr a, int depth = 0)
    {
        if (a is Term term)
            return Math.Max(DepthOf(term.L, depth + 1), DepthOf(term.R, depth + 1));
        return depth;
    }

    protected override void Verify(Article article)
    {
        Assert.Equal("DivideByZeroException: Attempted to divide by zero.", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(2, article.Total().Actions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(8, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Rethrow", article.Execution(1).Action(1).Read().Label);
        Assert.Equal("Run", article.Execution(1).Action(2).Read().Label);
        Assert.Equal("Input", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("( _ / ( 2 / _ ) )", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("( _ / ( 0 / _ ) )", article.Execution(1).Input(1).Read().Redux.Value);
    }
}
