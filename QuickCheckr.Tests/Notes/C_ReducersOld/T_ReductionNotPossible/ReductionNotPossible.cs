using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Notes.C_ReducersOld.T_ReductionNotPossible;

[DocFile]
[DocContent("What to do if all reduction values fail ?")]
public class ReductionNotPossible : CheckrTest<ReductionNotPossible>
{
    protected override bool Asserts => false;
    protected override bool Report => false;

    [Fact]
    [DocCheckrHeader]
    [DocCheckr]
    [DocReportHeader]
    [DocReport]
    public override void Example() => Run(1373577700);

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
         from input in Checkr.Input("Input", Fuzzr.Int(), Shrink.OnType(Reduce.TowardsZero))
         from expect in Checkr.Expect("Input should be greater than 42", () => input > 42)
         select Case.Closed;

    protected override void Verify(Article article)
    {
        Assert.Equal("Input should be greater than 42", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(1, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Input", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("21", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("0", article.Execution(1).Input(1).Read().Redux.Value);
    }
}
