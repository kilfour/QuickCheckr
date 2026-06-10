using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Tests.Tools;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Investigations.Z_ShrinkingBugs;

[DocFile]
[DocContent("Fixed this one by using Testimonys, see Memory.")]
public class A_GreedIsNotGood : CheckrTest<A_GreedIsNotGood>
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
        Run(() => TheCheckr().Run(1.ExecutionsPerRun()));

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from input in Checkr.Input("List", Fuzzr.Constant(new List<int>() { 1, 1 }))
        from length in Checkr.Expect("input.Count == 2", () => input.Count == 2)
        from expect in Checkr.ExpectWhen("does not contain 1", () => input.Count == 2, () => !input.Contains(1))
        select Case.Closed;

    protected override void Verify(Article article)
    {
        Assert.Equal("does not contain 1", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(1, article.Total().PassedExpectations());
        // Assert.Equal(1, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("List", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("[ _, 1 ]", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("input.Count == 2", article.PassedExpectation(1).Read().Label);
        Assert.Equal(1, article.PassedExpectation(1).Read().TimesPassed);
    }
}
