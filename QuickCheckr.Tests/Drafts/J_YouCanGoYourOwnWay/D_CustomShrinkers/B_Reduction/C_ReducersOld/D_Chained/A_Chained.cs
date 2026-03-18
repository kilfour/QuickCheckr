using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Drafts.J_YouCanGoYourOwnWay.D_CustomShrinkers.B_Reduction.C_ReducersOld.D_Chained;

// [DocFile]
public class A_Chained : CheckrTest<A_Chained>
{
    protected override bool Asserts => false;
    protected override bool Report => true;

    [Fact(Skip = "TODO")]
    [DocCheckrHeader]
    [DocCheckr]
    [DocReportHeader]
    [DocReport]
    public override void Example() => Run(1681332589);

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
         from input in Checkr.Input("Input", Fuzzr.Int(),
            Shrink.OnType<int>(s => s.Simplify(Reduce.Towards(5), Reduce.Towards(0))))
         from expect in Checkr.Expect("Input < 2", () => input < 2)
         select Case.Closed;

    protected override void Verify(Article article)
    {
        Assert.Equal("Input < 2", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(1, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Input", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("79", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("2", article.Execution(1).Input(1).Read().Redux.Value);
    }
}
