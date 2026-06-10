using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Tests.Tools;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Investigations.M_ExpectationMessages;

[DocFile]
public class B_ExpectWhenMessage : CheckrTest<B_ExpectWhenMessage>
{
    protected override bool Asserts => false;
    protected override bool Report => false;

    [Fact]
    [DocCheckrHeader]
    [DocCheckr]
    [DocReportHeader]
    [DocReport]
    public override void Example() => Run(() => TheCheckr().Run());

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from input in Checkr.Input("Answer", Fuzzr.Constant(42))
        from expect in Checkr.ExpectWhen("Always fails", () => true, () => false, () => $"answer = {input}")
        select Case.Closed;

    protected override void Verify(Article article)
    {
        Assert.Equal("Always fails", article.FailureDescription());
        Assert.Equal("answer = 42", article.FailingExpectationMessages()[0]);
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Warnings());
        Assert.Equal(1, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("All inputs were considered irrelevant.", article.Execution(1).Warning(1).Read().Value);
    }
}