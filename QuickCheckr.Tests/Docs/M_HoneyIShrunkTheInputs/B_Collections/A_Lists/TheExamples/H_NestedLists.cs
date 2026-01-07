using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Docs.M_HoneyIShrunkTheInputs.B_Collections.A_Lists.TheExamples;

[DocFile]
public class H_NestedLists : CheckrTest<H_NestedLists>
{
    protected override bool Asserts => false;
    protected override bool Report => false;

    [Fact]
    [DocCheckrHeader]
    [DocCheckr]
    [DocReportHeader]
    [DocReport] public override void Example() => Run(146712596);

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from input in Checkr.Input("List", Fuzzr.Int().Many(0, 3).Many(0, 3))
        from trace in Checkr.Trace("Original", () => input)
        from expect in Checkr.ExpectWhen("List[0][0] is not 42",
            () => input.Any() && input.First().Any(),
            () => input.First().First() != 42)
        select Case.Closed;

    protected override void Verify(Article article)
    {
        Assert.Equal("List[0][0] is not 42", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(1, article.Total().Traces());
        Assert.Equal(4, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("List", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("[ [ 42 ] ]", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("Original", article.Execution(1).Trace(1).Read().Label);
        Assert.Equal("[ [ 42, 1, 15 ], [ 17 ], [ 27, 9 ] ]", article.Execution(1).Trace(1).Read().Value);
    }
}
