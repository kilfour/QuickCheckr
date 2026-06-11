using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Protocol;
using QuickCheckr.Tests.Tools;
using QuickPulse.Explains;


namespace QuickCheckr.Tests.Drafts.B_PuttingACaseOnTheBoard.C_Configuration.N_ShrinkMode.Reference;

[DocFileHeader("ShrinkMode.Default")]
public class ShrinkModeDefault : CheckrTest<ShrinkModeDefault>
{
    protected override bool Asserts => false;
    protected override bool Report => false;

    [Fact]
    [DocConfigure]
    [DocContent("Includes `ShrinkMode.Executions` and `ShrinkMode.Inputs`.")]
    [DocReport]
    public void DefaultSetting()
    {
        Assert.True(ShrinkMode.Default.HasFlag(ShrinkMode.Executions));
        Assert.True(ShrinkMode.Default.HasFlag(ShrinkMode.Inputs));
    }

    [Fact]
    public override void Example()
        => Run(() => TheCheckr().Configure(a => Configure(a)).Run(2145473476, 3.ExecutionsPerRun()));

    [CodeExample]
    private CheckrConfig Configure(CheckrConfig config)
        => config with { ShrinkMode = ShrinkMode.Default };

    protected override CheckrOf<Case> TheCheckr()
        => A_ShrinkModeReference.GetCheckr();

    protected override void Verify(Article article)
    {
        Assert.Equal("Third Time Answer", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(2, article.Total().Inputs());
        Assert.Equal(1, article.Total().PassedExpectations());
        Assert.Equal(4, article.ShrinkCount);
        Assert.Equal(3, article.Execution(1).Read().ExecutionId);
        Assert.Equal("counter", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("3", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("Relevant", article.Execution(1).Input(2).Read().Label);
        Assert.Equal("66", article.Execution(1).Input(2).Read().Value);
        Assert.Equal("42", article.Execution(1).Input(2).Read().Redux.Value);
        Assert.Equal("Third Time Answer", article.PassedExpectation(1).Read().Label);
        Assert.Equal(2, article.PassedExpectation(1).Read().TimesPassed);
    }
}
