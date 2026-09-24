using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Tests.Tools;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Investigations.M_ExpectationMessages;

[DocFile]
public class D_ExpectWhenMessages : CheckrTest<D_ExpectWhenMessages>
{
    protected override bool Asserts => false;
    protected override bool Report => false;

    [Fact]
    [DocCheckr]
    [DocReport]
    public override void Example() => Document(a => a.Run(1183209470));

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from input in Checkr.Input("Answer", Fuzzr.Constant(42))
        from expect in Checkr.ExpectWhen(
            "Always fails",
            () => true,
            expectation => expectation.Fail($"answer = {input}", "second message"))
        select Case.Closed;

    protected override void Verify(Article article)
    {
        Assert.Equal("Always fails", article.FailureDescription());
        Assert.Equal(["answer = 42", "second message"], article.FailingExpectationMessages());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(0, article.Total().Warnings());
        Assert.Equal(1, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
    }
}
