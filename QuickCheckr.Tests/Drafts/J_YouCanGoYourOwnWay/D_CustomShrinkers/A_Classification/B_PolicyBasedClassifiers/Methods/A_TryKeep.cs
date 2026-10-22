using QuickCheckr.Shrinking;
using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Tests.Tools;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Drafts.J_YouCanGoYourOwnWay.D_CustomShrinkers.A_Classification.B_PolicyBasedClassifiers.Methods;

[DocFileHeader("Keep")]
[DocContent(
@"`Try.Keep` explicitly marks an execution as relevant and worth preserving during shrinking.
In most scenarios this is not strictly necessary, as QuickCheckr will naturally retain behaviour that contributes to failure.
`Try.Keep` mainly exists for completeness and for rare performance-tuning cases where you want to prevent the shrinker from exploring removals you already know are pointless.
It also serves as an explicit signal of intent: this step is considered essential evidence.")]
public class A_TryKeep : CheckrTest<A_TryKeep>
{
    protected override bool Asserts => false;
    protected override bool Report => false;

    [Fact]
    [DocCheckr]
    [DocReport]
    public override void Example() =>
        Document(a => a.Run(161541321, 2.ExecutionsPerRun()));

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from counter in Checkr.Input("counter", Fuzzr.Counter("counter"))
        from thing in Checkr.Input("Null Thing",
            Fuzzr.Constant<Thing>(null!),
            Shrinkr.For<Thing>().Classify(Try.Keep<Thing>()))
        from expect in Checkr.Expect("counter != 2", () => counter != 2)
        select Case.Closed;

    protected override void Verify(Article article)
    {
        Assert.Equal("counter != 2", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(2, article.Total().Inputs());
        Assert.Equal(1, article.Total().PassedExpectations());
        Assert.Equal(1, article.ShrinkCount);
        Assert.Equal(2, article.Execution(1).Read().ExecutionId);
        Assert.Equal("counter", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("2", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("Null Thing", article.Execution(1).Input(2).Read().Label);
        Assert.Equal("null", article.Execution(1).Input(2).Read().Value);
        Assert.Equal("counter != 2", article.PassedExpectation(1).Read().Label);
        Assert.Equal(1, article.PassedExpectation(1).Read().TimesPassed);
    }

    public class Thing;
}
