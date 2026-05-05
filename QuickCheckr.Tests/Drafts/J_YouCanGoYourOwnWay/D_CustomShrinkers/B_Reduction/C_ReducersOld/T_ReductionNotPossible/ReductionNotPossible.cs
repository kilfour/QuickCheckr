using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Tests.Tools;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Drafts.J_YouCanGoYourOwnWay.D_CustomShrinkers.B_Reduction.C_ReducersOld.T_ReductionNotPossible;

[DocFile]
public class ReductionNotPossible : CheckrTest<ReductionNotPossible>
{
    protected override bool Asserts => false;
    protected override bool Report => false;
    protected override bool Explain => false;

    [Fact]
    [DocCheckrHeader]
    [DocCheckr]
    [DocReportHeader]
    [DocReport]
    public override void Example() => Run(1373577700);

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
         from input in Checkr.Input("Input", Fuzzr.Int(),
            Shrink.OnType<int>(s => s.Simplify(Reduce.TowardsZero)))
         from expect in Checkr.Expect("Input should be greater than 42", () => input > 42)
         select Case.Closed;

    protected override void Verify(Article article)
    {
        Assert.Equal("Input should be greater than 42", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(1, article.Total().Warnings());
        Assert.Equal(1, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Input", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("21", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("0", article.Execution(1).Input(1).Read().Redux.Value);
        Assert.Equal("No witness for reducer at 'Input'.", article.Execution(1).Warning(1).Read().Value);
    }
}
