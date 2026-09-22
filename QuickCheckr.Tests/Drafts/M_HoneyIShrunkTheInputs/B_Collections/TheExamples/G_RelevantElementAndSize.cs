using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Tests.Tools;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Drafts.M_HoneyIShrunkTheInputs.B_Collections.TheExamples;

[DocFile]
public class G_RelevantElementAndSize : CheckrTest<G_RelevantElementAndSize>
{
    protected override bool Asserts => false;
    protected override bool Report => false;

    [Fact]
    [DocCheckr]
    [DocReport] public override void Example() => Document(a => a.Run(1374380958));

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from input in Checkr.Input("List", Fuzzr.Int(1, 10).Many(1, 10))
        from trace in Checkr.Trace("Original", a => a.Show(input))
        from expect in Checkr.ExpectWhen("Second element is not 7",
            () => input.Count() > 1, () => input.ElementAt(1) != 7)
        select Case.Closed;

    protected override void Verify(Article article)
    {
        Assert.Equal("Second element is not 7", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(1, article.Total().Traces());
        // Assert.Equal(5, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("List", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("[ _, 7 ]", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("Original", article.Execution(1).Trace(1).Read().Label);
        Assert.Equal(["[ 6, 7, 9, 7, 3, 2 ]"], article.Execution(1).Trace(1).Read().Value);
    }
}
