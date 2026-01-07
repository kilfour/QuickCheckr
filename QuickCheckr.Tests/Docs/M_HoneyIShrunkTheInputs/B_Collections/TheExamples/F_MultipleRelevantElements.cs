using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Docs.M_HoneyIShrunkTheInputs.B_Collections.TheExamples;

[DocFile]
public class F_MultipleRelevantElements : CheckrTest<F_MultipleRelevantElements>
{
    protected override bool Asserts => false;
    protected override bool Report => false;

    [Fact]
    [DocCheckrHeader]
    [DocCheckr]
    [DocReportHeader]
    [DocReport] public override void Example() => Run(500957855);

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from input in Checkr.Input("List", Fuzzr.Int(1, 10).Many(1, 10))
        from list in Checkr.Capture(() => input.ToList())
        from trace in Checkr.Trace("Original", () => list)
        from expect in Checkr.ExpectWhen("One, Two and Three", () => input.Count() > 2, () =>
             !(list[0] == 1
            && list[1] == 2
            && list[2] == 3))
        select Case.Closed;

    protected override void Verify(Article article)
    {
        Assert.Equal("One, Two and Three", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(1, article.Total().Traces());
        Assert.Equal(3, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("List", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("[ 1, 2, 3 ]", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("Original", article.Execution(1).Trace(1).Read().Label);
        Assert.Equal("[ 1, 2, 3, 9, 3, 1 ]", article.Execution(1).Trace(1).Read().Value);
    }
}