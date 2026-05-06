using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Tests.Tools;
using QuickFuzzr;
using QuickPulse.Explains;
using QuickPulse.Instruments;

namespace QuickCheckr.Tests.Investigations.Sequence;

[DocFile]
[DocContent("Fixed this one by using Testimonys, see Memory.")]
public class CheckrSequence : CheckrTest<CheckrSequence>
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
        Run(10.ExecutionsPerRun());

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from counter in Checkr.Input("Counter", Fuzzr.Counter("counter"))
        from sequence in Checkr.Sequence(
            Checkr.Act("1", () => { if (counter == 3) ComputerSays.No("BOOM"); }),
            Checkr.Act("2", () => { })
        )
        select Case.Closed;

    protected override void Verify(Article article)
    {
        Assert.Equal("ComputerSaysNo: BOOM", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Actions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(2, article.ShrinkCount);
        Assert.Equal(3, article.Execution(1).Read().ExecutionId);
        Assert.Equal("1", article.Execution(1).Action(1).Read().Label);
        Assert.Equal("Counter", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("3", article.Execution(1).Input(1).Read().Value);
    }
}
