using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Protocol;
using QuickCheckr.Tests.Tools;
using QuickPulse.Explains;


namespace QuickCheckr.Tests.Drafts.B_PuttingACaseOnTheBoard.C_Configuration.J_ReportMode.Reference;

[DocFileHeader("ReportMode.Default")]
public class ReportModeDefault : CheckrTest<ReportModeDefault>
{
    protected override bool Asserts => false;
    protected override bool Report => false;

    [Fact]
    [DocConfigure]
    [DocContent("Includes all possible ReportModes, except `ReportMode.FinalTrace`.")]
    [DocReport]
    public override void Example()
    {
        Assert.True(ReportMode.Default.HasFlag(ReportMode.Trace));
        Document(a => a.Configure(a => Configure(a)).Run(1964562926, 2.ExecutionsPerRun()));
    }

    [CodeExample]
    private CheckrConfig Configure(CheckrConfig config)
        => config with { ReportMode = ReportMode.Default };

    protected override CheckrOf<Case> TheCheckr()
        => A_ReportModeReference.GetCheckr();

    protected override void Verify(Article article)
    {
        Assert.Equal("ComputerSaysNo: Only one is available", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Actions());
        Assert.Equal(2, article.Total().Inputs());
        Assert.Equal(1, article.Total().Traces());
        Assert.Equal(2, article.ShrinkCount);
        Assert.Equal(2, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Blows up on 2", article.Execution(1).Action(1).Read().Label);
        Assert.Equal("Counter", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("2", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("Null Thing", article.Execution(1).Input(2).Read().Label);
        Assert.Equal("null", article.Execution(1).Input(2).Read().Value);
        Assert.Equal("Irrelevant", article.Execution(1).Trace(1).Read().Label);
        Assert.Equal("73", article.Execution(1).Trace(1).Read().Value);
    }
}
