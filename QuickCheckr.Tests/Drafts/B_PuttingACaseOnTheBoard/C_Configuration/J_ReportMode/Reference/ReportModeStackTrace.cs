using QuickCheckr.Protocol;
using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickPulse.Explains;


namespace QuickCheckr.Tests.Drafts.B_PuttingACaseOnTheBoard.C_Configuration.J_ReportMode.Reference;

[DocFileHeader("ReportMode.StackTrace")]
public class ReportModeStackTrace : CheckrTest<ReportModeStackTrace>
{
    protected override bool Asserts => false;
    protected override bool Report => false;

    [Fact]
    [DocConfigure]
    [DocContent("Includes the StackTrace in the failure description if an exception is thrown.")]
    [DocReport]
    public override void Example()
        => Run(1964562926, 2.ExecutionsPerRun());

    [CodeExample]
    protected override CheckrConfig Configure(CheckrConfig config)
        => config with { ReportMode = ReportMode.StackTrace };

    protected override CheckrOf<Case> TheCheckr()
        => A_ReportModeReference.GetCheckr();

    protected override void Verify(Article article)
    {
        Assert.Equal("ComputerSaysNo: Only one is available", article.FailureDescription());
        Assert.Contains("at QuickPulse.Instruments.ComputerSays.No", article.GetExceptionMessage());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Actions());
        Assert.Equal(2, article.Total().Inputs());
        Assert.Equal(2, article.ShrinkCount);
        Assert.Equal(2, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Blows up on 2", article.Execution(1).Action(1).Read().Label);
        Assert.Equal("Counter", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("2", article.Execution(1).Input(1).Read().Value);
        Assert.False(article.Execution(1).Input(1).Read().Labeled);
        Assert.Equal("Null Thing", article.Execution(1).Input(2).Read().Label);
        Assert.Equal("null", article.Execution(1).Input(2).Read().Value);
        Assert.False(article.Execution(1).Input(1).Read().Labeled);
    }
}
