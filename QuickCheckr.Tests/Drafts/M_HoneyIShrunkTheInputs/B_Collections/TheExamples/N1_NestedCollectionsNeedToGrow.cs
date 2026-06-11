using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Tests.Tools;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Drafts.M_HoneyIShrunkTheInputs.B_Collections.TheExamples;

[DocFile]
[DocContent(
@"Nested collections can in rare circumstances be classified as AssumedIrrelevant, but they might not be.")]
public class N1_NestedCollectionsNeedToGrow : CheckrTest<N1_NestedCollectionsNeedToGrow>
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
        => Document(a => a.Run(482835926));

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from input in Checkr.Input("Museum",
            from a in Fuzzr.Int().Many(3, 10)
            select new Museum(a))
        from trace in Checkr.Trace("Original", () => input)
        from expect in Checkr.Expect("List count >= 4", () => input.A.Count() >= 4)
        select Case.Closed;

    protected override void Verify(Article article)
    {
        Assert.Equal("List count >= 4", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Traces());
        Assert.Equal(1, article.Total().Warnings());
        Assert.Equal(2, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Original", article.Execution(1).Trace(1).Read().Label);
        Assert.Equal("{ A: [ 74, 41, 59 ] }", article.Execution(1).Trace(1).Read().Value);
        Assert.Equal("All inputs were considered irrelevant.", article.Execution(1).Warning(1).Read().Value);
    }

    public record Museum(IEnumerable<int> A);
}
