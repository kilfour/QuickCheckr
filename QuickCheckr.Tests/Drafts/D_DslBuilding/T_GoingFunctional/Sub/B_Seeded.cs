using QuickCheckr.Invariants;
using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickCheckr.UnderTheHood.Proceedings;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Drafts.D_DslBuilding.T_GoingFunctional.Sub;

[DocFile]
public class B_Seeded : InvariantsTest<B_Seeded>
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
    protected override CaseFile TheInvariant() =>
        Check.ForAll(1948674479, Fuzzr.Int(0, 10), a => a != 6);

    protected override void Verify(Article article)
    {
        Assert.Equal(1948674479, article.Seed());
        Assert.Equal("Property", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(0, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Input", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("6", article.Execution(1).Input(1).Read().Value);
    }
}
