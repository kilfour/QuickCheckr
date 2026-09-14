using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Tests.Tools;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Drafts.J_YouCanGoYourOwnWay.D_CustomShrinkers.B_Reduction.C_ReducersOld.A_Values;

[DocFile]
public class A_ValuesStopOnFirstPassed : CheckrTest<A_ValuesStopOnFirstPassed>
{
    protected override bool Asserts => false;
    protected override bool Report => false;

    [Fact]
    [DocCheckr]
    [DocReport]
    public override void Example() => Document(a => a.Run(1681332589));

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
         from input in Checkr.Input("Input", Fuzzr.Int(), Shrink.OnType<int>(s => s.Simplify(Reduce.Values([3, 2, 1, 0]))))
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
