using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Drafts.D_DslBuilding.F_ModelBasedTesting.Sub;

[DocFile]
public class A_UsingLinq : CheckrTest<A_UsingLinq>
{
    protected override bool Asserts => false;
    protected override bool Report => false;

    [Fact]
    [DocCheckrHeader]
    [DocCheckr]
    [DocReportHeader]
    [DocReport]
    public override void Example() =>
        Run(1299945702, 100.ExecutionsPerRun());

    [CodeSnippet]

    protected override CheckrOf<Case> TheCheckr() =>
        from model in Trackr.Stashed(() => new Model())
        from sut in Trackr.Stashed(() => new Sut())
        from ops in Checkr.OneOf(
            from input in Checkr.Input("Input", Fuzzr.Int())
            from add in Checkr.Act("Add", () =>
            {
                model.Add(input);
                sut.Add(input);
            })
            select Case.Closed,
            from input in Checkr.Input("Input", Fuzzr.Int())
            from add in Checkr.Act("Subtract", () =>
            {
                model.Subtract(input);
                sut.Subtract(input);
            })
            select Case.Closed,
            Checkr.Act("Clear", () =>
            {
                model.Clear();
                sut.Clear();
            })
        )
        from observe in Checkr.Expect("Observe", () => model.Result == sut.Result)
        from trace in Checkr.TraceWhen("Observation", () => model.Result != sut.Result, () => $"Model: {model.Result}, Sut: {sut.Result}.")
        select Case.Closed;

    protected override void Verify(Article article)
    {
        Assert.Equal("Observe", article.FailureDescription());
        Assert.Equal(2, article.Total().Executions());
        Assert.Equal(2, article.Total().Actions());
        Assert.Equal(1, article.Total().Traces());
        Assert.Equal(1, article.Total().Warnings());
        Assert.Equal(1, article.Total().PassedExpectations());
        Assert.Equal(13, article.ShrinkCount);
        Assert.Equal(4, article.Execution(1).Read().ExecutionId);
        Assert.Equal(3, article.Execution(1).Times);
        Assert.Equal("Add", article.Execution(1).Action(1).Read().Label);
        Assert.Equal("All inputs were considered irrelevant.", article.Execution(1).Warning(1).Read().Value);
        Assert.Equal(14, article.Execution(2).Read().ExecutionId);
        Assert.Equal("Clear", article.Execution(2).Action(1).Read().Label);
        Assert.Equal("Observation", article.Execution(2).Trace(1).Read().Label);
        Assert.Equal("Model: 0, Sut: 196.", article.Execution(2).Trace(1).Read().Value);
        Assert.Equal("Observe", article.PassedExpectation(1).Read().Label);
        Assert.Equal(13, article.PassedExpectation(1).Read().TimesPassed);
    }
}
