using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Tests.Tools;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Drafts.M_HoneyIShrunkTheInputs.B_Collections.TheExamples;


[DocFile]
public class B_RemovingIrrelevantCollection : CheckrTest<B_RemovingIrrelevantCollection>
{
    protected override bool Asserts => false;
    protected override bool Report => false;

    [Fact]
    [DocContent(
@"Ofcourse a collection itself is just an input like another, so if it, as a whole,
is irrelevant for the failure, QuickCheckr will remove it.")]
    [DocCheckrHeader]
    [DocCheckr]
    [DocReportHeader]
    [DocReport]
    public override void Example() => Run(2014608033);

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from listInput in Checkr.Input("List Input", Fuzzr.Int().Many(0, 10))
        from expect in Checkr.Expect("Always fails", () => false)
        select Case.Closed;

    protected override void Verify(Article article)
    {
        Assert.Equal("Always fails", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
    }
}