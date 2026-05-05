using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Tests.Tools;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Investigations;

[DocFile]
public class UncheckedExpectationsTest : CheckrTest<UncheckedExpectationsTest>
{
    protected override bool Asserts => false;
    protected override bool Report => false;
    protected override bool Explain => false;

    [Fact]
    [DocCheckrHeader]
    [DocCheckr]
    [DocReportHeader]
    [DocReport]
    public override void Example() =>
        Run(5.ExecutionsPerRun());

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from input in Checkr.Input("input", Fuzzr.Counter("count"))
        from expect in Checkr.ExpectWhen("checked", () => input > 1, () => false)
        from expectUnchecked in Checkr.ExpectWhen("unchecked", () => input > 2, () => false)
        select Case.Closed;

    protected override void Verify(Article article)
    {
        Assert.Equal("checked", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(1, article.Total().UncheckedExpectations());
        Assert.Equal(1, article.ShrinkCount);
        Assert.Equal(2, article.Execution(1).Read().ExecutionId);
        Assert.Equal("input", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("2", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("unchecked", article.UncheckedExpectation(1).Read().Label);
    }
}
