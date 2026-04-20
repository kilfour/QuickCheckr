using QuickCheckr.Protocol;
using QuickCheckr.Tests.Tools;
using QuickCheckr.Authoring.ThePress.Printing;
using QuickPulse.Explains;


namespace QuickCheckr.Tests.Drafts.B_PuttingACaseOnTheBoard.C_Configuration.N_ShrinkMode.Reference;

[DocFileHeader("ShrinkMode.Inputs")]
public class ShrinkModeInputs : CheckrTest<ShrinkModeInputs>
{
    protected override bool Asserts => false;
    protected override bool Report => false;

    [Fact]
    [DocConfigure]
    [DocContent("Trims inputs to find a minimal failing case.")]
    [DocReport]
    public override void Example()
        => Run(2145473476, 3.ExecutionsPerRun());

    [CodeExample]
    protected override CheckrConfig Configure(CheckrConfig config)
        => config with { ShrinkMode = ShrinkMode.Inputs };

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
        Assert.Equal("Relevant", article.Execution(2).Input(2).Read().Label);
        Assert.Equal("66", article.Execution(2).Input(2).Read().Value);
        Assert.Equal("Third Time Answer", article.PassedExpectation(1).Read().Label);
        Assert.Equal(2, article.PassedExpectation(1).Read().TimesPassed);
    }
}
