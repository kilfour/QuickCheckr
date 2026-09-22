using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Tests.Tools;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Investigations.M_ExpectationMessages;

[DocFile]
public class F_VerifyMessages : CheckrTest<F_VerifyMessages>
{
    protected override bool Asserts => false;
    protected override bool Report => false;

    [Fact]
    [DocCheckr]
    [DocReport]
    public override void Example() => Document(a => a.Run());

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from input in Checkr.Input("Answer", Fuzzr.Constant(42))
        from verify in Trackr.Verify(
            "Always fails",
            expectation => expectation.Fail($"answer = {input}", "second message"))
        select Case.Closed;

    protected override void Verify(Article article)
    {
        Assert.Equal("Always fails", article.VerifyFailed());
        Assert.Equal(["answer = 42", "second message"], article.FailingExpectationMessages());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(0, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
    }
}
