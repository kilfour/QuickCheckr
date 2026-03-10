using QuickCheckr.Invariants;
using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Drafts.D_DslBuilding.T_GoingFunctional.Sub;

[DocFile]
public class G_WithClassifiersSeededShowingReduxToo : InvariantsTest<G_WithClassifiersSeededShowingReduxToo>
{
    protected override bool Asserts => false;
    protected override bool Report => false;

    [Fact]
    [DocInvariantHeader]
    [DocInvariant]
    [DocReportHeader]
    [DocReport]
    public override void Example() => Run();

    [CodeSnippet]
    protected override void TheInvariant() =>
        Check.ForAll(893841622, Fuzzr.Int(0, 10), a => a == 6, Try.Default<int>(), Try.Values(6));

    protected override void Verify(Article article)
    {
        Assert.Equal("Property", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(1, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Input", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("9", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("7", article.Execution(1).Input(1).Read().Redux.Value);
    }
}