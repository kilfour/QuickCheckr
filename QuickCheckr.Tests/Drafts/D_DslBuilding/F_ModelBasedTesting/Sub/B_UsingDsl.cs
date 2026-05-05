using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Tests.Tools;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Drafts.D_DslBuilding.F_ModelBasedTesting.Sub;

[DocFile]
public class B_UsingDsl : CheckrTest<B_UsingDsl>
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
        Run(1879928073, 20.ExecutionsPerRun());

    [CodeSnippet]
    [CodeRemove("return ")]
    protected override CheckrOf<Case> TheCheckr()
    {
        return Check
            .Model(() => new CalculatorModel())
            .Sut(() => new Calculator())
            .Operation("Add", Fuzzr.Int(),
                (model, a) => model.Add(a),
                (sut, a) => sut.Add(a))
            .Operation("Subtract", Fuzzr.Int(),
                (model, a) => model.Subtract(a),
                (sut, a) => sut.Subtract(a))
            .Operation("Clear",
                model => model.Clear(),
                sut => sut.Clear())
            .Report((model, sut) => $"Model: {model.Result}, Sut: {sut.Result}.")
            .Observe((model, sut) => model.Result == sut.Result);
    }

    protected override void Verify(Article article)
    {
        Assert.Equal("Observe", article.FailureDescription());
        Assert.Equal(2, article.Total().Executions());
        Assert.Equal(2, article.Total().Actions());
        Assert.Equal(1, article.Total().Traces());
        Assert.Equal(1, article.Total().Warnings());
        Assert.Equal(1, article.Total().PassedExpectations());
        Assert.Equal(6, article.ShrinkCount);
        Assert.Equal(2, article.Execution(1).Read().ExecutionId);
        Assert.Equal(3, article.Execution(1).Times);
        Assert.Equal("Add", article.Execution(1).Action(1).Read().Label);
        Assert.Equal("All inputs were considered irrelevant.", article.Execution(1).Warning(1).Read().Value);
        Assert.Equal(7, article.Execution(2).Read().ExecutionId);
        Assert.Equal("Clear", article.Execution(2).Action(1).Read().Label);
        Assert.Equal("Observation", article.Execution(2).Trace(1).Read().Label);
        Assert.Equal("Model: 0, Sut: 193.", article.Execution(2).Trace(1).Read().Value);
        Assert.Equal("Observe", article.PassedExpectation(1).Read().Label);
        Assert.Equal(6, article.PassedExpectation(1).Read().TimesPassed);
    }
}
