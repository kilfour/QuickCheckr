using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Investigations.F_ObjectInputPrinting.Sub;

[DocFile]
public class D_CustomizedWithIrrelevant : CheckrTest<D_CustomizedWithIrrelevant>
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
            new Term { L = new Num { Value = 84 }, R = new Num { Value = 0 }, Op = Operation.Div }))
        from trace in Checkr.Trace("Original", () => expr)
        from eval in Checkr.Act("Evaluate", expr.Evaluate)
        select Case.Closed;

    protected override void Verify(Article article)
    {
        Assert.Equal("( _ / 0 )", article.Execution(1).Input(1).Read().Value);
    }
}