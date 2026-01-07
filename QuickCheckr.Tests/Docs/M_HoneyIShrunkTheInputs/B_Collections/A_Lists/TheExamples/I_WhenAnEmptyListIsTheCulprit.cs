using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Docs.M_HoneyIShrunkTheInputs.B_Collections.A_Lists.TheExamples;

[DocFile]
public class I_WhenAnEmptyListIsTheCulprit : CheckrTest<I_WhenAnEmptyListIsTheCulprit>
{
    protected override bool Asserts => false;
    protected override bool Report => false;

    [Fact]
    [DocCheckrHeader]
    [DocCheckr]
    [DocReportHeader]
    [DocReport] public override void Example() => Run(2077876521);

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from list in Checkr.Input("List", Fuzzr.Int().Many(0, 10))
        from trace in Checkr.Trace("Original", () => list)
        from listNotEmpty in Checkr.Expect("List is not empty", () => list.Any())
        select Case.Closed;

    protected override void Verify(Article article)
    {
        Assert.Equal("List is not empty", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(1, article.Total().Traces());
        Assert.Equal(0, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("List", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("[ ]", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("Original", article.Execution(1).Trace(1).Read().Label);
        Assert.Equal("[ ]", article.Execution(1).Trace(1).Read().Value);
    }
}
