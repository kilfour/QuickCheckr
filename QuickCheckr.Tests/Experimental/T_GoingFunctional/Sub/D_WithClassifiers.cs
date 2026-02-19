using QuickCheckr.Invariants;
using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Experimental.T_GoingFunctional.Sub;

[DocFile]
public class D_WithClassifiers : InvariantsTest<D_WithClassifiers>
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
        Check.ForAll(Fuzzr.Constant(7), a => a == 6, Try.Default<int>(), Try.Values(6));

    protected override void Verify(Article article)
    {
        Assert.Equal("Property", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(0, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Input", article.Execution(1).Input(1).Read().Label);
        Assert.NotEmpty(article.Execution(1).Input(1).Read().Value.ToString()!);
    }
}