using QuickCheckr;
using QuickCheckr.Authoring.ThePress;
using QuickCheckr.Authoring.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;
using QuickTestr.Tests.Tools;

namespace QuickTestr.Tests.Docs.C_ModelBasedTesting.Sub;

[DocFile]
[DocModelHeader]
[DocExample(typeof(IdentityCounterModel))]
[DocBoldHeader("SUT")]
[DocExample(typeof(IdentityCounter))]
[DocTestrHeader]
[DocTestr]
[DocReportHeader]
[DocReport]
public class E_CheckingTheResultsAsWell : QuickTestrModelRunTest<E_CheckingTheResultsAsWell>
{
    protected override bool Asserts => false;
    protected override bool Report => false;
    protected override bool Explain => false;

    [Fact]
    public override void Example() => Document();

    [CodeSnippet]
    [CodeRemoveJournalist]
    [CodeRemove("520188124, 50.ExecutionsPerRun()")]
    protected override void GetTestr(Journalist journalist) =>
        Testr.Named("IdentityCounter matches model")
            .Model(() => new IdentityCounterModel())
            .Sut(() => new IdentityCounter())
            .VerifyOperationResults()
            .Operation("Do", Fuzzr.Int(),
                (model, a) => model.Do(a),
                (sut, a) => sut.Do(a))
            .Observe("Counter Matches",
                (model, sut) => model.Counter == sut.Counter)
            .StoreCaseFiles(journalist)
            .Run(520188124, 50.ExecutionsPerRun());

    protected override void Verify(Article article)
    {
        Assert.Equal("Do, results do not match", article.FailureDescription());
        Assert.Equal("", article.VerifyFailed());
        Assert.Equal(520188124, article.Seed());
        Assert.Equal(4, article.Total().Executions());
        Assert.Equal(8, article.Total().Actions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(8, article.Total().Traces());
        Assert.Equal("-QTM-Do", article.Execution(4).Action(1).Read().Label);
        Assert.Equal("-QTS-Do", article.Execution(4).Action(2).Read().Label);
        Assert.Equal("78", article.Execution(4).Input(1).Read().Value);
        Assert.Equal("Expected", article.Execution(4).Trace(1).Read().Label);
        Assert.Equal("78", article.Execution(4).Trace(1).Read().Value);
        Assert.Equal("Actual  ", article.Execution(4).Trace(2).Read().Label);
        Assert.Equal("0", article.Execution(4).Trace(2).Read().Value);
    }
}

[CodeExample]
public class IdentityCounterModel
{
    public int Counter { get; private set; }
    public int Do(int a)
    {
        Counter++;
        return a;
    }
}

[CodeExample]
public class IdentityCounter
{
    public int Counter { get; private set; }
    public int Do(int a)
    {
        Counter++;
        if (Counter > 3)
            return 0;
        return a;
    }
}
