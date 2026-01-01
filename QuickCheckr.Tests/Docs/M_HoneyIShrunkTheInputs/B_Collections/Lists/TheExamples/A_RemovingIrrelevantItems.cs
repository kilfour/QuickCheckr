using QuickCheckr.Protocol;
using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Docs.M_HoneyIShrunkTheInputs.B_Collections.Lists.TheExamples;

[DocFile]
public class A_RemovingIrrelevantItems : CheckrTest<A_RemovingIrrelevantItems>
{
    protected override bool Asserts => false;
    protected override bool Report => false;
    protected override bool Explanation => false;

    [Fact]
    [DocContent("Consider a test that forbids the value `7`.")]
    [DocCheckrHeader]
    [DocCheckr]
    [DocReportHeader]
    [DocReport]
    [DocContent(@"Even if the original list was long and varied, QuickCheckr isolates the single relevant element:")]
    public override void Example() => Run(1833095082);

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from listInput in Checkr.Input("List Input", Fuzzr.Int().Many(0, 10))
        from expect in Checkr.Expect("List does not contain 7", () => !listInput.Contains(7))
        from trace in Checkr.Trace("List Input", () => listInput)
        select Case.Closed;

    protected override void Verify(Article article)
    {
        Assert.Equal("List does not contain 7", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(1, article.Total().Traces());
        Assert.Equal(8, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("List Input", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("[ 7 ]", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("List Input", article.Execution(1).Trace(1).Read().Label);
        Assert.Equal("[ 69, 49, 90, 36, 16, 71, 95, 7, 84 ]", article.Execution(1).Trace(1).Read().Value);
    }
}