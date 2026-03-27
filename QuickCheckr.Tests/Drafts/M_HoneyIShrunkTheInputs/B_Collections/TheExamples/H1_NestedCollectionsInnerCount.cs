using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Drafts.M_HoneyIShrunkTheInputs.B_Collections.TheExamples;

[DocFile]
public class H1_NestedCollectionsInnerCount : CheckrTest<H1_NestedCollectionsInnerCount>
{
    protected override bool Asserts => false;
    protected override bool Report => false;
    protected override bool Explain => false;

    [Fact]
    [DocCheckrHeader]
    [DocCheckr]
    [DocReportHeader]
    [DocReport]
    public override void Example()
        => Run(1860909202);

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from input in Checkr.Input("List", Fuzzr.Int().Many(1, 5).Many(3, 10))
        from trace in Checkr.Trace("Original", () => input)
        from expect in Checkr.Expect("List count <= 4", () => input.Count() <= 4)
        select Case.Closed;

    protected override void Verify(Article article)
    {
        Assert.Equal("List count <= 4", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(1, article.Total().Traces());
        Assert.Equal(5, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("List", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("[ _, _, _, _, _ ]", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("Original", article.Execution(1).Trace(1).Read().Label);
        Assert.Equal("[ [ 34, 19 ], [ 62, 47, 14, 22, 13 ], [ 64 ], [ 42, 5, 78, 63, 94 ], [ 4, 7, 91, 94 ] ]", article.Execution(1).Trace(1).Read().Value);
    }
}
