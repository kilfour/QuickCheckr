using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Tests.Tools;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Drafts.M_HoneyIShrunkTheInputs.B_Collections.TheExamples;

[DocFile]
[DocContent(
@"*Top level collections reuse the fuzzr from Input to make this work.  
For nested collections, more work/thinking is needed.*")]
public class N_CollectionsNeedToGrow : CheckrTest<N_CollectionsNeedToGrow>
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
        => Run(() => TheCheckr().Run(482835926));

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
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(1, article.Total().Traces());
        Assert.Equal(3, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("List", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("[ ]", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("Original", article.Execution(1).Trace(1).Read().Label);
        Assert.Equal("[ 74, 41, 59 ]", article.Execution(1).Trace(1).Read().Value);
    }
}
