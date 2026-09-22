using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Tests.Tools;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Drafts.M_HoneyIShrunkTheInputs.C_Objects.TheExamples;

[DocFile]
public class L_TuplesAgain : CheckrTest<L_TuplesAgain>
{
    protected override bool Asserts => false;
    protected override bool Report => false;
    protected override bool Explain => false;

    [Fact]
    [DocCheckr]
    [DocReport]
    public override void Example() =>
        Document(a => a.Run(751254928));

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from input in Checkr.Input("Tuple",
            from a in Fuzzr.Int()
            from b in Fuzzr.Int()
            from c in Fuzzr.Int()
            select Tuple.Create(a, b, c),
            Shrink.OnType<int>(s => s.Simplify(Reduce.TowardsZero)))
        from irrelevant in Checkr.Input("Irrelevant",
            from a in Fuzzr.Int()
            from b in Fuzzr.Int()
            select Tuple.Create(a, b))
        from traceOne in Checkr.Trace("Original", a => a.Show(input))
        from expect in Checkr.Expect("Sum is < 42.", () => input.Item1 + input.Item2 < 42)
        select Case.Closed;

    protected override void Verify(Article article)
    {
        Assert.Equal("Sum is < 42.", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(1, article.Total().Traces());
        Assert.Equal(4, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Tuple", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("( 52, 25, _ )", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("( 17, 25, _ )", article.Execution(1).Input(1).Read().Redux.Value);
        Assert.Equal("Original", article.Execution(1).Trace(1).Read().Label);
        Assert.Equal(["{ Item1: 52, Item2: 25, Item3: 23 }"], article.Execution(1).Trace(1).Read().Value);
    }
}