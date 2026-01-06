using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Drafts.G_CustomShrinkers;

[DocFile]
public class ShrinkMultiple : CheckrTest<ShrinkMultiple>
{
    protected override bool Asserts => false;
    protected override bool Report => true;
    protected override bool Explanation => false;

    [Fact(Skip = "WIP")]
    [DocCheckrHeader]
    [DocCheckr]
    [DocReportHeader]
    [DocReport]
    public override void Example() =>
        Run(161541321, 2.ExecutionsPerRun());

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from counter in Checkr.Input("counter", Fuzzr.Counter("counter"))
        from thingOne in Checkr.Input("Null Thing One",
            Fuzzr.Constant<Thing>(null!),
            // The second one will remove this value from the report.
            // The first one will still emit a warning.
            () => [Shrink.Default<Thing>(), Shrink.Values([new Thing()])])
        from thingTwo in Checkr.Input("Null Thing Two",
            Fuzzr.Constant<Thing>(null!),
            // Now the first one will shortcircuit shrinking, so no warning from the second one.
            () => [Shrink.Values([new Thing()]), Shrink.Default<Thing>()])
        from expect in Checkr.Expect("counter != 2", () => counter != 2)
        select Case.Closed;

    protected override void Verify(Article article)
    {
        Assert.Equal("counter != 2", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(1, article.Total().Warnings());
        Assert.Equal(1, article.Total().PassedExpectations());
        Assert.Equal(3, article.ShrinkCount);
        Assert.Equal(2, article.Execution(1).Read().ExecutionId);
        Assert.Equal("counter", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("2", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("Null value for type 'Thing' at input 'Null Thing One'.", article.Execution(1).Warning(1).Read().Value);
        Assert.Equal("counter != 2", article.PassedExpectation(1).Read().Label);
        Assert.Equal(1, article.PassedExpectation(1).Read().TimesPassed);
    }

    public class Thing;
}
