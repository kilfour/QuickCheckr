using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Drafts.G_CustomShrinkers.Sub;

[DocFile]
public class ShrinkFuzzr : CheckrTest<ShrinkFuzzr>
{
    protected override bool Asserts => false;
    protected override bool Report => false;

    [Fact]
    [DocCheckrHeader]
    [DocCheckr]
    [DocReportHeader]
    [DocReport]
    public override void Example() =>
        Run(161541321, 2.ExecutionsPerRun());

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from counter in Checkr.Input("counter", Fuzzr.Counter("counter"))
        from thing in Checkr.Input("Null Thing", Fuzzr.Constant<Thing>(null!), () => Shrink.Fuzzr(Fuzzr.One<Thing>(), 2))
        from expect in Checkr.Expect("counter != 2", () => counter != 2)
        select Case.Closed;

    protected override void Verify(Article article)
    {
        Assert.Equal("counter != 2", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(1, article.Total().PassedExpectations());
        Assert.Equal(2, article.ShrinkCount);
        Assert.Equal(2, article.Execution(1).Read().ExecutionId);
        Assert.Equal("counter", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("2", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("counter != 2", article.PassedExpectation(1).Read().Label);
        Assert.Equal(1, article.PassedExpectation(1).Read().TimesPassed);
    }

    public class Thing;
}
