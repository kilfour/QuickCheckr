using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Tests.Tools;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Drafts.J_YouCanGoYourOwnWay.N_Showr.Reference.Sub;

[DocFile]
public class B_Customized : CheckrTest<B_Customized>
{
    protected override bool Asserts => false;
    protected override bool Report => false;
    protected override bool Explain => false;

    [Fact]
    [DocCheckr]
    [DocReport]
    public override void Example() =>
        Document(a => a.Run(1611568796));

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
            new Term { L = new Num { Value = 84 }, R = new Num { Value = 2 }, Op = Operation.Div }))
        from trace in Checkr.Trace("Original", a => a.Show(expr))
        from eval in Checkr.Act("Evaluate", expr.Evaluate)
        from expect in Checkr.Expect("Not 42", () => eval != 42)
        select Case.Closed;

    protected override void Verify(Article article)
    {
        Assert.Equal("( 84 / 2 )", article.Execution(1).Input(1).Read().Value);
    }
}