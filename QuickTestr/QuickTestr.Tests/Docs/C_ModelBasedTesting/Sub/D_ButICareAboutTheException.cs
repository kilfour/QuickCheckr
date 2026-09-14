using QuickCheckr;
using QuickCheckr.Authoring.ThePress;
using QuickCheckr.Authoring.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;
using QuickTestr.Tests.Tools;

namespace QuickTestr.Tests.Docs.C_ModelBasedTesting.Sub;

[DocFile]
[DocContent(
"""
When activating *strict mode* a mismatch in exceptions do fail the model test.
"""
)]
[DocModelHeader]
[DocExample(typeof(NameCollectorModel))]
[DocBoldHeader("SUT")]
[DocExample(typeof(NameCollector))]
[DocTestrHeader]
[DocTestr]
[DocReportHeader]
[DocReport]
public class D_ButICareAboutTheException : QuickTestrModelRunTest<D_ButICareAboutTheException>
{
    protected override bool Asserts => false;
    protected override bool Report => false;
    protected override bool Explain => false;

    [Fact]
    public override void Example() => Document();

    [CodeSnippet]
    [CodeRemoveJournalist]
    [CodeRemove("1830780673, 50.ExecutionsPerRun()")]
    protected override void GetTestr(Journalist journalist) =>
        Testr.Named("NameCollector matches model")
            .Model(() => new NameCollectorModel())
            .Sut(() => new NameCollector())
            .VerifyOperationResults()
            .Operation("Add", Fuzzr.String(1),
                (model, a) => model.Add(a),
                (sut, a) => sut.Add(a))
            .Observe("Result Matches",
                (model, sut) => model.Names.SequenceEqual(sut.Names), a => a.Trace())
            .StoreCaseFiles(journalist)
            .Run(1830780673, 50.ExecutionsPerRun());

    protected override void Verify(Article article)
    {
        Assert.Equal("Add, results do not match", article.FailureDescription());
        Assert.Equal("", article.VerifyFailed());
        Assert.Equal(1830780673, article.Seed());
        Assert.Equal(2, article.Total().Executions());
        Assert.Equal(4, article.Total().Actions());
        Assert.Equal(2, article.Total().Inputs());
        Assert.Equal(1, article.Total().Traces());
        Assert.Equal("-QTM-Add", article.Execution(2).Action(1).Read().Label);
        Assert.Equal("-QTS-Add", article.Execution(2).Action(2).Read().Label);
        Assert.Equal("\"k\"", article.Execution(2).Input(1).Read().Value);
        Assert.Equal("Actual  ", article.Execution(2).Trace(1).Read().Label);
        Assert.Equal(
            "ComputerSaysNo: Already have that one ...",
            article.Execution(2).Trace(1).Read().Value);
    }
}

