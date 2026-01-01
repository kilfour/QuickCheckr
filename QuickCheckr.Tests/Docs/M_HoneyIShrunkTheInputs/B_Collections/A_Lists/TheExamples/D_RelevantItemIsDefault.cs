using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Docs.M_HoneyIShrunkTheInputs.B_Collections.A_Lists.TheExamples;

[DocFile]
public class D_RelevantItemIsDefault : CheckrTest<D_RelevantItemIsDefault>
{
    protected override bool Asserts => false;
    protected override bool Report => false;
    protected override bool Explanation => false;

    [Fact]
    [DocContent("Just making sure `default` values, `0` in this case, get handled correctly.")]
    [DocCheckrHeader]
    [DocCheckr]
    [DocReportHeader]
    [DocReport] public override void Example() => Run(820833320);

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from listInput in Checkr.Input("List Input", Fuzzr.Int(0, 100).Many(0, 10))
        from expect in Checkr.Expect("List does not contain 0", () => !listInput.Contains(0))
        from trace in Checkr.Trace("List Input", () => listInput)
        select Case.Closed;

    protected override void Verify(Article article)
    {
        Assert.Equal("List does not contain 0", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(1, article.Total().Traces());
        Assert.Equal(8, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("List Input", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("[ 0 ]", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("List Input", article.Execution(1).Trace(1).Read().Label);
        Assert.Equal("[ 84, 63, 90, 91, 55, 53, 39, 91, 0 ]", article.Execution(1).Trace(1).Read().Value);
    }
}
