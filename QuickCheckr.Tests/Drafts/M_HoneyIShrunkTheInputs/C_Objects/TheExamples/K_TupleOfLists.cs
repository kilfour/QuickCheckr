using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Tests.Tools;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Drafts.M_HoneyIShrunkTheInputs.C_Objects.TheExamples;

[DocFile]
public class K_TupleOfLists : CheckrTest<K_TupleOfLists>
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
        Run(1892439403);

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from input in Checkr.Input("Tuple",
            from a in Fuzzr.Int().Many(2)
            from b in Fuzzr.Int().Many(2)
            from c in Fuzzr.Int().Many(0)
            select (a, b, c),
            Shrink.OnType<int>(s => s.Simplify(Reduce.TowardsZero)))
        from irrelevant in Checkr.Input("Irrelevant",
            from a in Fuzzr.Int()
            from b in Fuzzr.Int()
            select (a, b))
        from traceOne in Checkr.Trace("Original", () => input)
        from expect in Checkr.Expect("First list does not contain 42.", () => !input.a.Contains(42))
        select Case.Closed;

    protected override void Verify(Article article)
    {
        Assert.Equal("First list does not contain 42.", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(1, article.Total().Traces());
        Assert.Equal(3, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Tuple", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("( [ 42 ], _, _ )", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("Original", article.Execution(1).Trace(1).Read().Label);
        Assert.Equal("( [ 42, 32 ], [ 17, 69 ], [ ] )", article.Execution(1).Trace(1).Read().Value);
    }
}