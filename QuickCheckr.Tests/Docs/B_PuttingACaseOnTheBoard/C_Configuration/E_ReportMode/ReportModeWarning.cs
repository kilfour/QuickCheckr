using QuickCheckr.Protocol;
using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickPulse.Explains;


namespace QuickCheckr.Tests.Docs.B_PuttingACaseOnTheBoard.C_Configuration.E_ReportMode;

[DocFileHeader("ReportMode.Warning")]
public class ReportModeWarning : CheckrTest<ReportModeWarning>
{
    protected override bool Asserts => false;
    protected override bool Report => false;
    protected override bool Explanation => false;

    [Fact]
    [DocConfigure]
    [DocContent("Only warnings.")]
    [DocReport]
    public override void Example()
        => Run(1964562926, 2.ExecutionsPerRun(), Configure);

    [CodeExample]
    protected override CheckrConfig Configure(CheckrConfig config)
        => config with { ReportMode = ReportMode.Warning };

    protected override CheckrOf<Case> TheCheckr()
        => ReportModeTests.GetCheckr();

    protected override void Verify(Article article)
    {
        Assert.Equal("ComputerSaysNo: Only one is available", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Actions());
        Assert.Equal(2, article.Total().Inputs());
        Assert.Equal(1, article.Total().Warnings());
        Assert.Equal(2, article.ShrinkCount);
        Assert.Equal(2, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Blows up on 2", article.Execution(1).Action(1).Read().Label);
        Assert.Equal("Counter", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("2", article.Execution(1).Input(1).Read().Value);
        Assert.False(article.Execution(1).Input(1).Read().Labeled);
        Assert.Equal("Null Thing", article.Execution(1).Input(2).Read().Label);
        Assert.Equal("null", article.Execution(1).Input(2).Read().Value);
        Assert.False(article.Execution(1).Input(1).Read().Labeled);
        Assert.Equal("Null value for type 'Thing' at input 'Null Thing'.", article.Execution(1).Warning(1).Read().Value);
    }
}