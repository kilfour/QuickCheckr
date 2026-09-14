using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Tests.Tools;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Drafts.M_HoneyIShrunkTheInputs.C_Objects.TheExamples;

[DocFile]
public class Q_ObjectOfLists : CheckrTest<Q_ObjectOfLists>
{
    protected override bool Asserts => false;
    protected override bool Report => false;
    protected override bool Explain => false;

    [Fact]
    [DocCheckr]
    [DocReport]
    public override void Example() =>
        Document(a => a.Run(1622405126));

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from input in Checkr.Input("Museum",
            from a in Fuzzr.Int().Many(2).ToList()
            from b in Fuzzr.Int().Many(2).ToList()
            from c in Fuzzr.Int().Many(0).ToList()
            select new Museum(a, b, c),
            Shrink.OnType<int>(s => s.Simplify(Reduce.TowardsZero)))
        from irrelevant in Checkr.Input("Irrelevant",
            from a in Fuzzr.Int()
            from b in Fuzzr.Int()
            select (a, b))
        from traceOne in Checkr.Trace("Original", () => input)
        from expect in Checkr.Expect("First list does not contain 42.", () => !input.A.Contains(42))
        select Case.Closed;

    protected override void Verify(Article article)
    {
        Assert.Equal("First list does not contain 42.", article.FailureDescription());
        Assert.Equal("", article.VerifyFailed());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(1, article.Total().Traces());
        Assert.Equal(6, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Museum", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("{ A: [ 42 ] }", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("Original", article.Execution(1).Trace(1).Read().Label);
        Assert.Equal("{ A: [ 42, 6 ], B: [ 53, 25 ], C: [ ] }", article.Execution(1).Trace(1).Read().Value);
    }

    public record Museum(List<int> A, List<int> B, List<int> C);
}