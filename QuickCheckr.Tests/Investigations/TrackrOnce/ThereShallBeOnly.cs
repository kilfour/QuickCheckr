using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Tests.Tools;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Investigations.TrackrOnce;

[DocFile]
public class ThereShallBeOnly : CheckrTest<ThereShallBeOnly>
{
    protected override bool Asserts => false;
    protected override bool Report => false;
    protected override bool Explain => false;

    [Fact]
    [DocCheckr]
    [DocReport]
    public override void Example() =>
        Document(a => a.Run(10.ExecutionsPerRun()));

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from counter1 in Checkr.Input("Counter-1", Fuzzr.Counter("counter-1"))
        from expect1 in Checkr.Expect("counter1 <= 5", () => counter1 <= 5)
        from once in Trackr.Once("Only Once",
            from counter2 in Checkr.Input("Counter-2", Fuzzr.Counter("counter-2"))
            from expect2 in Checkr.Expect("counter2 <= 1", () => counter2 <= 1)
            select Case.Closed)
        select Case.Closed;

    protected override void Verify(Article article)
    {
        Assert.Equal("counter1 <= 5", article.FailureDescription());
        Assert.Equal("", article.VerifyFailed());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(2, article.Total().PassedExpectations());
        Assert.Equal(5, article.ShrinkCount);
        Assert.Equal(6, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Counter-1", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("6", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("counter1 <= 5", article.PassedExpectation(1).Read().Label);
        Assert.Equal(5, article.PassedExpectation(1).Read().TimesPassed);
        Assert.Equal("counter2 <= 1", article.PassedExpectation(2).Read().Label);
        Assert.Equal(1, article.PassedExpectation(2).Read().TimesPassed);
    }
}
