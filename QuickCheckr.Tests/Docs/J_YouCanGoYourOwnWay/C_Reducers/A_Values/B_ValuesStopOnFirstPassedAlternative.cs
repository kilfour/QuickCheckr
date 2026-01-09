using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Docs.J_YouCanGoYourOwnWay.C_Reducers.A_Values;

[DocFile]
public class B_ValuesStopOnFirstPassedAlternative : CheckrTest<B_ValuesStopOnFirstPassedAlternative>
{
    protected override bool Asserts => false;
    protected override bool Report => false;

    [Fact(Skip = "Wip")]
    [DocCheckrHeader]
    [DocCheckr]
    [DocReportHeader]
    [DocReport]
    public override void Example() => Run(1681332589);

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
         from input in Checkr.Input("Input", Fuzzr.Int(), Reduce.Values([3, 0, 1, 2]))
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
        Assert.Equal("3", article.Execution(1).Input(1).Read().Redux.Value);
    }
}
