using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Tests.Tools;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Investigations.M_ExpectationMessages;

[DocFile]
public class A_ExpectMessage : CheckrTest<A_ExpectMessage>
{
    protected override bool Asserts => false;
    protected override bool Report => false;

    [Fact]
    [DocCheckr]
    [DocReport]
    public override void Example() => Document(a => a.Run(435330806));

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from input in Checkr.Input("Answer", Fuzzr.Constant(42))
        from expect in Checkr.Expect("Always fails", () => false, () => $"answer = {input}")
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
