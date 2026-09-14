using QuickCheckr;
using QuickCheckr.Authoring.ThePress;
using QuickCheckr.Authoring.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;
using QuickTestr.Tests.Tools;

namespace QuickTestr.Tests.Docs.C_ModelBasedTesting.Sub;

[DocFile]
[DocModelHeader]
[DocExample(typeof(CalculatorModel))]
[DocBoldHeader("SUT")]
[DocExample(typeof(Calculator))]
[DocTestrHeader]
[DocTestr]
[DocReportHeader]
[DocReport]
public class A_CalculatorMemory : QuickTestrModelRunTest<A_CalculatorMemory>
{
    protected override bool Asserts => false;
    protected override bool Report => true;
    protected override bool Explain => false;

    [Fact]
    public override void Example() => Document();

    [CodeSnippet]
    [CodeRemoveJournalist]
    [CodeRemove("1626335899, 20.ExecutionsPerRun()")]
    protected override void GetTestr(Journalist journalist) =>
        Testr.Named("Calculator Clear matches model")
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
            .Observe("Result Matches",
                (model, sut) => model.Result == sut.Result, a => a.Trace())
            .StoreCaseFiles(journalist)
            .Run(1626335899, 20.ExecutionsPerRun());

    protected override void Verify(Article article)
    {
        Assert.Equal("Result Matches", article.FailureDescription());
        Assert.Equal("", article.VerifyFailed());
        Assert.Equal(2, article.Total().Executions());
        Assert.Equal(4, article.Total().Actions());
        Assert.Equal(2, article.Total().Traces());
        Assert.Equal(1, article.Total().Warnings());
        Assert.Equal(1, article.Total().PassedExpectations());
        Assert.Equal(16, article.ShrinkCount);
        Assert.Equal(2, article.Execution(1).Read().ExecutionId);
        Assert.Equal(3, article.Execution(1).Times);
        Assert.Equal("-QTM-Add", article.Execution(1).Action(1).Read().Label);
        Assert.Equal("-QTS-Add", article.Execution(1).Action(2).Read().Label);
        Assert.Equal("All inputs were considered irrelevant.", article.Execution(1).Warning(1).Read().Value);
        Assert.Equal(17, article.Execution(2).Read().ExecutionId);
        Assert.Equal("-QTM-Clear", article.Execution(2).Action(1).Read().Label);
        Assert.Equal("-QTS-Clear", article.Execution(2).Action(2).Read().Label);
        Assert.Equal("Model:", article.Execution(2).Trace(1).Read().Label);
        Assert.Equal("{ Result: 0 }", article.Execution(2).Trace(1).Read().Value);
        Assert.Equal("Sut:  ", article.Execution(2).Trace(2).Read().Label);
        Assert.Equal("{ Result: 124 }", article.Execution(2).Trace(2).Read().Value);
        Assert.Equal("Result Matches", article.PassedExpectation(1).Read().Label);
        Assert.Equal(16, article.PassedExpectation(1).Read().TimesPassed);
    }
}


[CodeExample]
public class CalculatorModel
{
    public int Result { get; private set; } = 0;
    public void Add(int a) => Result += a;
    public void Subtract(int a) => Result -= a;
    public void Clear() => Result = 0;
}

[CodeExample]
public class Calculator
{
    public int Result { get; private set; } = 0;
    private int counter = 0;
    public void Add(int a)
    {
        counter++;
        Result += a;
    }

    public void Subtract(int a) => Result -= a;
    public void Clear()
    {
        if (counter != 3)
            Result = 0;
    }
}
