using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Tests.Tools;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Notes.A_Shrinking.C_InputShrinking.B_Collections;

[DocFile]
public class E_RelevantItemIsAllButDefault : CheckrTest<E_RelevantItemIsAllButDefault>
{
    protected override bool Asserts => false;
    protected override bool Report => false;

    [Fact]
    [DocContent("Just making sure `default` values, `0` in this case, get handled correctly.")]
    [DocCheckr]
    [DocReport] public override void Example() => Document(a => a.Run(816312044));

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from listInput in Checkr.Input("List Input", Fuzzr.Int(0, 100).Many(2, 10))
        from expect in Checkr.Expect("List contains only 0", () => listInput.All(a => a == 0))
        from trace in Checkr.Trace("List Input", () => listInput)
        select Case.Closed;

    protected override void Verify(Article article)
    {
        Assert.Equal("List contains only 0", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(1, article.Total().Traces());
        // Assert.Equal(8, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("List Input", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("[ 51 ]", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("List Input", article.Execution(1).Trace(1).Read().Label);
        Assert.Equal("[ 28, 0, 50, 27, 37, 54, 74, 56, 51 ]", article.Execution(1).Trace(1).Read().Value);
    }
}