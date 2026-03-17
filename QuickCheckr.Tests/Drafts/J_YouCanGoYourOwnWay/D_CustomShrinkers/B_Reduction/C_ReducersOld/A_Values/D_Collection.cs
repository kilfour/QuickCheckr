using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Drafts.J_YouCanGoYourOwnWay.D_CustomShrinkers.B_Reduction.C_ReducersOld.A_Values;

[DocFile]
[DocContent("Helper function in order to avoid generic type confusion.")]
public class D_Collection : CheckrTest<D_Collection>
{
    protected override bool Asserts => false;
    protected override bool Report => false;

    [Fact]
    [DocCheckrHeader]
    [DocCheckr]
    [DocReportHeader]
    [DocReport]
    public override void Example() => Run(1681332589);

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
         from input in Checkr.Input("Input", Fuzzr.Int().Many(1, 5),
            Shrink.OnType<IEnumerable<int>>(s => s.Simplify(Reduce.Collection([[3], [2], [1], [0]]))))
         from expect in Checkr.Expect("All < 2", () => input.All(a => a < 2))
         select Case.Closed;

    protected override void Verify(Article article)
    {
        Assert.Equal("All < 2", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(4, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Input", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("[ 94 ]", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("[ 2 ]", article.Execution(1).Input(1).Read().Redux.Value);
    }
}
