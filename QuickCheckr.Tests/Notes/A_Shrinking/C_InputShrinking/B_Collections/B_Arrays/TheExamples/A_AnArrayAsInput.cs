using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Notes.A_Shrinking.C_InputShrinking.B_Collections.B_Arrays.TheExamples;

[DocFile]
public class A_AnArrayAsInput : CheckrTest<A_AnArrayAsInput>
{
    protected override bool Asserts => false;
    protected override bool Report => false;

    [Fact]
    [DocContent(
@"Seeing as all inputs are provided by QuickFuzzr,
and the default generated enumerable underlying type is a list,
we have to cheat a bit using a `Fuzzr.Constant()` in order to pass in an Array.")]
    [DocCheckrHeader]
    [DocCheckr]
    [DocReportHeader]
    [DocReport]
    public override void Example() => Run(27506614);

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from input in Checkr.Input("int[]", Fuzzr.Constant(new int[] { 42, 2, 3 }))
        from trace in Checkr.Trace("Original", () => input)
        from expect in Checkr.Expect("No answer here",
            () => !input.Contains(42))
        select Case.Closed;

    protected override void Verify(Article article)
    {
        Assert.Equal("No answer here", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(1, article.Total().Traces());
        // Assert.Equal(2, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("int[]", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("[ 42 ]", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("Original", article.Execution(1).Trace(1).Read().Label);
        Assert.Equal("[ 42, 2, 3 ]", article.Execution(1).Trace(1).Read().Value);
    }
}