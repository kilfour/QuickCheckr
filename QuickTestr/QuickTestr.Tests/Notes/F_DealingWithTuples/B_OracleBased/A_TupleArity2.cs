using QuickTestr.Tests.Tools;
using QuickFuzzr;
using QuickPulse.Explains;
using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Authoring.ThePress;

namespace QuickTestr.Tests.Notes.F_DealingWithTuples.B_OracleBased;

[DocFile]
public class A_TupleArity2 : QuickTestrOracleTest<A_TupleArity2>
{
    protected override bool Asserts => false;
    protected override bool Report => false;
    protected override bool Explain => false;

    [Fact]
    [DocTestrHeader]
    [DocTestr]
    [DocReportHeader]
    [DocReport]
    public override void Example() => Document();

    [CodeSnippet]
    [CodeRemoveJournalist]
    [CodeRemove("649859307")]
    protected override void GetTestr(Journalist journalist) =>
        Testr.Named("Tuple2 can deconstruct")
            .For(Fuzzr.Int(), Fuzzr.Int())
            .Expected((a, b) => a)
            .Actual((a, b) => b)
            .StoreCaseFiles(journalist)
            .Run(649859307);

    protected override void Verify(Article article)
    {
        Assert.Equal("Tuple2 can deconstruct", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(2, article.Total().Actions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(2, article.Total().Traces());
        Assert.Equal(2, article.Total().FinalTraces());
        Assert.Equal(1, article.Total().Warnings());
        Assert.Equal(1, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Actual", article.Execution(1).Action(1).Read().Label);
        Assert.Equal("Expected", article.Execution(1).Action(2).Read().Label);
        Assert.Equal("Input", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("( _, 31 )", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("( 37, 31 )", article.Execution(1).Input(1).Read().Original.Value);
        Assert.Equal("( _, 0 )", article.Execution(1).Input(1).Read().Redux.Value);
        Assert.Equal("Expected", article.Execution(1).Trace(1).Read().Label);
        Assert.Equal("37", article.Execution(1).Trace(1).Read().Value);
        Assert.Equal("Actual  ", article.Execution(1).Trace(2).Read().Label);
        Assert.Equal("31", article.Execution(1).Trace(2).Read().Value);
        Assert.Equal("Expected", article.Execution(1).FinalTrace(1).Read().Label);
        Assert.Equal("37", article.Execution(1).FinalTrace(1).Read().Value);
        Assert.Equal("Actual  ", article.Execution(1).FinalTrace(2).Read().Label);
        Assert.Equal("0", article.Execution(1).FinalTrace(2).Read().Value);
        Assert.Equal("No witness for reducer at 'Input.Item2'.", article.Execution(1).Warning(1).Read().Value);
    }
}
