using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Notes.A_Shrinking.C_InputShrinking.B_Collections.T_ListContainingNullValues;

[DocFile]
public class ListContainingNullValues : CheckrTest<ListContainingNullValues>
{
    protected override bool Asserts => false;
    protected override bool Report => false;

    [Fact]
    [DocCheckrHeader]
    [DocCheckr]
    [DocReportHeader]
    [DocReport]
    public override void Example() => Run(1833095082);

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from listInput in Checkr.Input("List Input", Fuzzr.Int().Nullable().Many(0, 10))
        from expect in Checkr.Expect("no nulls allowed", () => listInput.All(a => a.HasValue))
        from trace in Checkr.Trace("List Input", () => listInput)
        select Case.Closed;

    protected override void Verify(Article article)
    {
        Assert.Equal("no nulls allowed", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(1, article.Total().Traces());
        // Assert.Equal(8, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("List Input", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("[ null ]", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("List Input", article.Execution(1).Trace(1).Read().Label);
        Assert.Equal("[ 49, 36, null, 95, null, 82, 73, 58, 2 ]", article.Execution(1).Trace(1).Read().Value);
    }
}