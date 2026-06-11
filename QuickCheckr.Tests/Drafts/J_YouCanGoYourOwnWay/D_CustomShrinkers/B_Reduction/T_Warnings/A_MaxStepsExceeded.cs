using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Protocol;
using QuickCheckr.Tests.Tools;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Drafts.J_YouCanGoYourOwnWay.D_CustomShrinkers.B_Reduction.T_Warnings;

[DocFile]
public class A_MaxStepsExceeded : CheckrTest<A_MaxStepsExceeded>
{
    protected override bool Asserts => false;
    protected override bool Report => false;
    protected override bool Explain => false;

    [Fact]
    [DocCheckrHeader]
    [DocCheckr]
    [DocConfigureHeader]
    [DocConfigure]
    [DocReportHeader]
    [DocReport]
    public override void Example() => Run(() => TheCheckr().Configure(a => Configure(a)).Run(1719639579));

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
         from input in Checkr.Input("Input", Fuzzr.Constant(42))
         from expect in Checkr.Expect("Input <= 0", () => input <= 0)
         select Case.Closed;

    [CodeSnippet]
    private CheckrConfig Configure(CheckrConfig config)
        => config with { MaxStepsPerReducer = 10, ShrinkMode = config.ShrinkMode | ShrinkMode.Reduction };

    protected override void Verify(Article article)
    {
        Assert.Equal("Input <= 0", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(1, article.Total().Warnings());
        Assert.Equal(1, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Input", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("42", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("30", article.Execution(1).Input(1).Read().Redux.Value);
        Assert.Equal("Reducer stopped at Input. Max steps (10) exceeded.", article.Execution(1).Warning(1).Read().Value);
    }
}
