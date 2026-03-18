using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Drafts.J_YouCanGoYourOwnWay.D_CustomShrinkers.B_Reduction.C_ReducersOld.K_BuiltInReducers;

[DocFile]
public class B_ReduceMinimizeString : CheckrTest<B_ReduceMinimizeString>
{
    protected override bool Asserts => false;
    protected override bool Report => false;

    [Fact]
    [DocCheckrHeader]
    [DocCheckr]
    [DocReportHeader]
    [DocReport]
    public override void Example() => Run(1834440365);

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from text in Checkr.Input("Text", Fuzzr.String(),
            Shrink.OnType<string>(s => s.Simplify(Reduce.MinimizeString)))
        from expect in Checkr.Expect("no a and b", () => !(text.Contains('a') && text.Contains('b')))
        select Case.Closed;

    protected override void Verify(Article article)
    {
        Assert.Equal("no a and b", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(1, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Text", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("\"tdblaulb\"", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("\"ab\"", article.Execution(1).Input(1).Read().Redux.Value);
    }
}
