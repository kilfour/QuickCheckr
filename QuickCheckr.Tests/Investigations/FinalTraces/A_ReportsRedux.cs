using QuickCheckr.Protocol;
using QuickCheckr.Tests.Tools;
using QuickCheckr.Authoring.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Investigations.G_GatheringEvidence;

[DocFile]
public class A_ReportsRedux : CheckrTest<A_ReportsRedux>
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
        Run(1883509239);

    protected override CheckrConfig Configure(CheckrConfig config)
        => config with { ReportMode = config.ReportMode | ReportMode.FinalTrace };

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from input in Checkr.Input("Input", Fuzzr.Int(), Shrink.OnType<int>(s => s.Simplify(Reduce.TowardsZero)))
        from trace in Checkr.Trace("Input", () => input)
        from expect in Checkr.Expect("<= 42", () => input <= 42)
        select Case.Closed;

    protected override void Verify(Article article)
    {
        Assert.Equal("<= 42", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(1, article.Total().Traces());
        Assert.Equal(1, article.Total().FinalTraces());
        Assert.Equal(1, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Input", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("69", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("43", article.Execution(1).Input(1).Read().Redux.Value);
        Assert.Equal("Input", article.Execution(1).Trace(1).Read().Label);
        Assert.Equal("69", article.Execution(1).Trace(1).Read().Value);
        Assert.Equal("Input", article.Execution(1).FinalTrace(1).Read().Label);
        Assert.Equal("43", article.Execution(1).FinalTrace(1).Read().Value);
    }
}
