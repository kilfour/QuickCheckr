using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Tests.Tools;
using QuickPulse.Bolts;
using QuickPulse.Explains;
using QuickPulse.Instruments;

namespace QuickCheckr.Tests.Investigations.Sequence;

[DocFile]
public class CheckrSequenceAllNeeded : CheckrTest<CheckrSequenceAllNeeded>
{
    protected override bool Asserts => false;
    protected override bool Report => false;
    protected override bool Explain => false;

    [Fact]
    [DocCheckr]
    [DocReport]
    public override void Example() =>
        Document(a => a.Run(10.ExecutionsPerRun()));

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from number in Trackr.Stashed(() => new Cell<int>(0))
        from sequence in Checkr.Sequence(
            Checkr.Act("1", () => { if (number.Value == 3) ComputerSays.No("BOOM"); number.Value = number.Value + 1; }),
            Checkr.Act("2", () => { number.Value = number.Value + 2; })
        )
        select Case.Closed;

    protected override void Verify(Article article)
    {
        Assert.Equal("ComputerSaysNo: BOOM", article.FailureDescription());
        Assert.Equal("", article.VerifyFailed());
        Assert.Equal(3, article.Total().Executions());
        Assert.Equal(3, article.Total().Actions());
        Assert.Equal(0, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("1", article.Execution(1).Action(1).Read().Label);
        Assert.Equal(2, article.Execution(2).Read().ExecutionId);
        Assert.Equal("2", article.Execution(2).Action(1).Read().Label);
        Assert.Equal(3, article.Execution(3).Read().ExecutionId);
        Assert.Equal("1", article.Execution(3).Action(1).Read().Label);
    }
}
