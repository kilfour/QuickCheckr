using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Tests.Tools;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Drafts.M_HoneyIShrunkTheInputs.B_Collections.TheExamples;

[DocFile]
[DocContent(
@"Adding a Fuzzr based classification is a quick way to verify that the collection was indeed irrelevant.

Adding a reducer as well, just to check whether the empty list also fails.")]
public class N2_NestedCollectionsNeedToGrow : CheckrTest<N2_NestedCollectionsNeedToGrow>
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
        => Run(482835926);

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from fuzzr in Checkr.Capture(() => Fuzzr.Int().Many(3, 10))
        from input in Checkr.Input("Museum",
            from a in fuzzr
            select new Museum(a),
            Shrink.OnType<IEnumerable<int>>(
                s => s.Classify(
                    Try.Default<IEnumerable<int>>(),
                    Try.Fuzzr(fuzzr)),
                s => s.Simplify(Reduce.Values<IEnumerable<int>>([[]]))))
        from trace in Checkr.Trace("Original", () => input)
        from expect in Checkr.Expect("List count >= 4", () => input.A.Count() >= 4)
        select Case.Closed;

    protected override void Verify(Article article)
    {
        Assert.Equal("List count >= 4", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(1, article.Total().Traces());
        Assert.Equal(1, article.Total().Warnings());
        Assert.Equal(2, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Museum", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("{ A: [ 74, 41, 59 ] }", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("{ A: [ ] }", article.Execution(1).Input(1).Read().Redux.Value);
        Assert.Equal("Original", article.Execution(1).Trace(1).Read().Label);
        Assert.Equal("{ A: [ 74, 41, 59 ] }", article.Execution(1).Trace(1).Read().Value);
        Assert.Equal("No witness for reducer at 'Museum.A'.", article.Execution(1).Warning(1).Read().Value);
    }

    public record Museum(IEnumerable<int> A);
}
