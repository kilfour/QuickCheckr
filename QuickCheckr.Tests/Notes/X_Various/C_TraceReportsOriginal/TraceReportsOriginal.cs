using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Notes.X_Various.C_TraceReportsOriginal;

[DocFile]
public class TraceReportsOriginal : CheckrTest<TraceReportsOriginal>
{
    protected override bool Asserts => false;
    protected override bool Report => false;

    [Fact]
    [DocCheckrHeader]
    [DocCheckr]
    [DocReportHeader]
    [DocReport]
    public override void Example() =>
        Run(1468295090);

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from x in Checkr.Input("x", Fuzzr.Int(-100, 100), Shrink.OnType<int>(s => s.Simplify(Reduce.TowardsZero)))
        from trace in Checkr.Trace("x (trace)", () => x)
        from expect in Checkr.Expect("x must be negative", () => x >= 0)
        select Case.Closed;

    protected override void Verify(Article article)
    {
        Assert.Equal("x must be negative", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(1, article.Total().Traces());
        Assert.Equal(1, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("x", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("-12", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("-1", article.Execution(1).Input(1).Read().Redux.Value);
        Assert.Equal("x (trace)", article.Execution(1).Trace(1).Read().Label);
        Assert.Equal("-12", article.Execution(1).Trace(1).Read().Value);
    }
}