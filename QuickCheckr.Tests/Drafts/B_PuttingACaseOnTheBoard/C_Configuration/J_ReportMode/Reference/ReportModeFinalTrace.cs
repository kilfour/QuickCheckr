using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Protocol;
using QuickCheckr.Tests.Tools;
using QuickFuzzr;
using QuickPulse.Explains;


namespace QuickCheckr.Tests.Drafts.B_PuttingACaseOnTheBoard.C_Configuration.J_ReportMode.Reference;

[DocFileHeader("ReportMode.FinalTrace")]
[DocContent(
"""
Using a different checkr in order to demonstrate the difference between `ReportMode.Trace` and `ReportMode.FinalTrace`.
- `ReportMode.Trace` reports the values of the initial run.
- `ReportMode.FinalTrace` reports the values of the final run, after shrinking.
""")]
public class ReportModeFinalTrace : CheckrTest<ReportModeFinalTrace>
{
    protected override bool Asserts => false;
    protected override bool Report => false;

    [Fact]
    [DocConfigure]
    [DocReport]
    public override void Example()
        => Run(() => TheCheckr().Configure(a => Configure(a)).Run(1964562926, 2.ExecutionsPerRun()));

    [CodeExample]
    private CheckrConfig Configure(CheckrConfig config)
        => config with { ReportMode = ReportMode.FinalTrace };

    protected override CheckrOf<Case> TheCheckr() =>
        from input in Checkr.Input("Input", Fuzzr.Int(),
            Shrink.OnType<int>(s => s.Simplify(Reduce.TowardsZero)))
        from trace in Checkr.Trace("Input / 2", () => input / 2)
        from expect in Checkr.Expect("Input > 10", () => input < 10)
        select Case.Closed;

    protected override void Verify(Article article)
    {
        Assert.Equal("Input > 10", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(1, article.Total().FinalTraces());
        Assert.Equal(1, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Input", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("59", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("10", article.Execution(1).Input(1).Read().Redux.Value);
        Assert.Equal("Input / 2", article.Execution(1).FinalTrace(1).Read().Label);
        Assert.Equal("5", article.Execution(1).FinalTrace(1).Read().Value);
    }
}
