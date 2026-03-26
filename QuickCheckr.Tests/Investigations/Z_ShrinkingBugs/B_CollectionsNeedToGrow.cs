using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Investigations.Z_ShrinkingBugs;

[DocFile]
[DocContent(
@"Not necessarily a bug, but not a helpful report either.")]
public class B_CollectionsNeedToGrow : CheckrTest<B_CollectionsNeedToGrow>
{
    protected override bool Asserts => false;
    protected override bool Report => false;
    protected override bool Explain => false;

    [Fact]
    [DocCheckrHeader]
    [DocCheckr]
    [DocReportHeader]
    [DocReport] public override void Example() => Run(100.Runs());

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from input in Checkr.Input("List", Fuzzr.Int().Many(3, 10))
        from trace in Checkr.Trace("Original", () => input)
        from expect in Checkr.Expect("List count >= 4", () => input.Count() >= 4)
        select Case.Closed;

    protected override void Verify(Article article)
    {
        Assert.Equal("List count >= 4", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Traces());
        Assert.Equal(2, article.Total().Warnings());
        Assert.Equal(1, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Original", article.Execution(1).Trace(1).Read().Label);
        Assert.Equal("[ 74, 41, 59 ]", article.Execution(1).Trace(1).Read().Value);
        Assert.Equal("Empty collection deemed irrelevant at input 'List'.", article.Execution(1).Warning(1).Read().Value);
        Assert.Equal("All inputs were considered irrelevant.", article.Execution(1).Warning(2).Read().Value);
    }
}
