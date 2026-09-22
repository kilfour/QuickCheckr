using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Tests.Tools;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Drafts.M_HoneyIShrunkTheInputs.B_Collections.TheExamples;

[DocFile]
[DocContent(
@"Default shrinking does not sample the input Fuzzr to regrow a collection.

An explicit non-empty witness makes the collection's relevance deterministic.")]
public class N_CollectionsNeedToGrow : CheckrTest<N_CollectionsNeedToGrow>
{
    protected override bool Asserts => false;
    protected override bool Report => false;
    protected override bool Explain => false;

    [Fact]
    [DocCheckr]
    [DocReport]
    public override void Example()
        => Document(a => a.Run(482835926));

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from input in Checkr.Input("List", Fuzzr.Int().Many(3, 10),
            Shrink.Values(new List<int> { 0, 0, 0, 0 }))
        from trace in Checkr.Trace("Original", a => a.Show(input))
        from expect in Checkr.Expect("List count >= 4", () => input.Count() >= 4)
        select Case.Closed;

    protected override void Verify(Article article)
    {
        Assert.Equal("List count >= 4", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(1, article.Total().Traces());
        Assert.Equal(0, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("List", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("[ 74, 41, 59 ]", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("Original", article.Execution(1).Trace(1).Read().Label);
        Assert.Equal(["[ 74, 41, 59 ]"], article.Execution(1).Trace(1).Read().Value);
    }
}
