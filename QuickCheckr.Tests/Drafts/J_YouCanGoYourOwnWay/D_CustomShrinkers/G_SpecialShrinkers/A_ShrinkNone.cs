using QuickCheckr.Tests.Tools;
using QuickCheckr.Authoring.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Drafts.J_YouCanGoYourOwnWay.D_CustomShrinkers.G_SpecialShrinkers;

[DocFileCodeHeader("Shrink.None<Thing>()")]
public class A_ShrinkNone : CheckrTest<A_ShrinkNone>
{
    protected override bool Asserts => false;
    protected override bool Report => false;

    [Fact]
    [DocCheckrHeader]
    [DocCheckr]
    [DocContent(
@"`Shrink.None` is a custom shrinker that explicitly marks an input as irrelevant and therefore not worth shrinking.
It is primarily a performance tool: when a branch is known not to influence the outcome,
`Shrink.None` prevents QuickCheckr from wasting time shrinking or re-executing it. 
This can significantly speed up scenarios that involve expensive operations such as WebAPI calls.
It can also be used to suppress unnecessary ""all inputs irrelevant"" warnings when that irrelevance is intentional and understood.
")]
    [DocReportHeader]
    [DocReport]
    public override void Example() =>
        Run(161541321, 2.ExecutionsPerRun());

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from counter in Checkr.Input("counter", Fuzzr.Counter("counter"))
        from thing in Checkr.Input("Null Thing",
            Fuzzr.Constant<Thing>(null!),
            Shrink.None<Thing>())
        from expect in Checkr.Expect("counter != 2", () => counter != 2)
        select Case.Closed;

    protected override void Verify(Article article)
    {
        Assert.Equal("counter != 2", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(1, article.Total().PassedExpectations());
        Assert.Equal(1, article.ShrinkCount);
        Assert.Equal(2, article.Execution(1).Read().ExecutionId);
        Assert.Equal("counter", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("2", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("counter != 2", article.PassedExpectation(1).Read().Label);
        Assert.Equal(1, article.PassedExpectation(1).Read().TimesPassed);
    }

    public class Thing;
}
