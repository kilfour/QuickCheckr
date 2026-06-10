using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Protocol;
using QuickCheckr.Tests.Tools;
using QuickPulse.Explains;


namespace QuickCheckr.Tests.Drafts.B_PuttingACaseOnTheBoard.C_Configuration.N_ShrinkMode.Reference;

[DocFileHeader("ShrinkMode.Reduction")]
public class ShrinkModeReduction : CheckrTest<ShrinkModeReduction>
{
    protected override bool Asserts => false;
    protected override bool Report => false;

    [Fact]
    [DocConfigure]
    [DocContent(
@"Activates the default built in reducers for all inputs.  
It only makes sense to activate this if ShrinkMode.Inputs is enabled.  
This option is disabled by default.")]
    [DocReport]
    public override void Example()
        => Run(() => TheCheckr().Run(2145473476, 3.ExecutionsPerRun(), Configure));

    [CodeExample]
    private CheckrConfig Configure(CheckrConfig config)
        => config with { ShrinkMode = ShrinkMode.Inputs | ShrinkMode.Reduction };

    protected override CheckrOf<Case> TheCheckr()
        => A_ShrinkModeReference.GetCheckr();

    protected override void Verify(Article article)
    {
        Assert.Equal("Third Time Answer", article.FailureDescription());
        Assert.Equal(2, article.Total().Executions());
        Assert.Equal(2, article.Total().Inputs());
        Assert.Equal(1, article.Total().Warnings());
        Assert.Equal(1, article.Total().PassedExpectations());
        // Assert.Equal(8, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal(2, article.Execution(1).Times);
        Assert.Equal("All inputs were considered irrelevant.", article.Execution(1).Warning(1).Read().Value);
        Assert.Equal(3, article.Execution(2).Read().ExecutionId);
        Assert.Equal("counter", article.Execution(2).Input(1).Read().Label);
        Assert.Equal("3", article.Execution(2).Input(1).Read().Value);
        Assert.Null(article.Execution(2).Input(1).Read().Redux.Value);
        Assert.Equal("Relevant", article.Execution(2).Input(2).Read().Label);
        Assert.Equal("66", article.Execution(2).Input(2).Read().Value);
        Assert.Equal("42", article.Execution(2).Input(2).Read().Redux.Value);
        Assert.Equal("Third Time Answer", article.PassedExpectation(1).Read().Label);
        Assert.Equal(2, article.PassedExpectation(1).Read().TimesPassed);
    }
}
