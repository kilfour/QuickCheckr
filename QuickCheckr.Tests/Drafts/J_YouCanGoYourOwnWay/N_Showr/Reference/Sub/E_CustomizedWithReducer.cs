using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Tests.Tools;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Drafts.J_YouCanGoYourOwnWay.N_Showr.Reference.Sub;

[DocFile]
public class E_CustomizedWithReducer : CheckrTest<E_CustomizedWithReducer>
{
    protected override bool Asserts => false;
    protected override bool Report => false;
    protected override bool Explain => false;

    [Fact]
    [DocCheckrHeader]
    [DocCheckr]
    [DocReportHeader]
    [DocReport]
    public override void Example() =>
        Run(1611568796);

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
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
        from expr in Checkr.Input("Expr", Fuzzr.Constant(
            new Term
            {
                L = new Num { Value = 84 },
                R = new Term { L = new Num { Value = 10 }, R = new Num { Value = 20 }, Op = Operation.Div },
                Op = Operation.Div
            }),
            Shrink.OnType<int>(s => s.Simplify(Reduce.Towards(1))))
        from trace in Checkr.Trace("Original", () => expr)
        from eval in Checkr.Act("Evaluate", expr.Evaluate)
        from expect in Checkr.Expect("Not 42", () => eval != 42)
        select Case.Closed;

    protected override void Verify(Article article)
    {
        Assert.Equal("( _ / ( 10 / 20 ) )", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("( _ / ( 1 / 2 ) )", article.Execution(1).Input(1).Read().Redux.Value);
    }
}