using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Protocol;
using QuickCheckr.Tests.Tools;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Drafts.J_YouCanGoYourOwnWay.D_CustomShrinkers.A_Classification.B_PolicyBasedClassifiers.Methods;


[DocFileHeader("Default")]
[DocContent(
@"`Try.Keep` applies QuickCheckr's normal relevance detection as one strategy within a classifier chain.
Classifiers are executed in succession, so Try.Default can be placed anywhere in the sequence.
If it finds a relevant candidate, the chain stops. Otherwise, the next classifier is attempted.
This makes it possible to combine engine-driven relevance detection with more opinionated strategies such as Try.Values(...)
without hard-coding one as the global default.")]
public class C_TryDefault : CheckrTest<C_TryDefault>
{
    protected override bool Asserts => false;
    protected override bool Report => false;

    [Fact]
    [DocConfigureHeader]
    [DocConfigure]
    [DocCheckrHeader]
    [DocCheckr]
    [DocReportHeader]
    [DocReport]
    public override void Example() =>
        Run(() => TheCheckr().Run(161541321, 2.ExecutionsPerRun(), Configure));

    [CodeSnippet]
    private CheckrConfig Configure(CheckrConfig config)
        => config with { WarningLevel = WarningLevel.Verbose };

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from counter in Checkr.Input("counter", Fuzzr.Counter("counter"))
        from thing in Checkr.Input("Null Thing",
            Fuzzr.Constant<Thing>(null!),
            Shrink.OnType<Thing>(s => s.Classify(Try.Default<Thing>())))
        from expect in Checkr.Expect("counter != 2", () => counter != 2)
        select Case.Closed;

    protected override void Verify(Article article)
    {
        Assert.Equal("counter != 2", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(2, article.Total().Inputs());
        Assert.Equal(1, article.Total().Warnings());
        Assert.Equal(1, article.Total().PassedExpectations());
        Assert.Equal(1, article.ShrinkCount);
        Assert.Equal(2, article.Execution(1).Read().ExecutionId);
        Assert.Equal("counter", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("2", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("Null Thing", article.Execution(1).Input(2).Read().Label);
        Assert.Equal("null", article.Execution(1).Input(2).Read().Value);
        Assert.Equal("Null value for type 'Thing' at input 'Null Thing'.", article.Execution(1).Warning(1).Read().Value);
        Assert.Equal("counter != 2", article.PassedExpectation(1).Read().Label);
        Assert.Equal(1, article.PassedExpectation(1).Read().TimesPassed);
    }

    public class Thing;
}
