using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;
using QuickPulse.Show;

namespace QuickCheckr.Tests.Drafts.J_YouCanGoYourOwnWay.N_Showr.Reference;


public class E_ForAll : CheckrTest<E_ForAll>
{
    protected override bool Asserts => false;
    protected override bool Report => false;

    [Fact]
    [DocCheckrHeader]
    [DocCheckr]
    [DocReportExcerptHeader]
    [DocReportExcerpt(6, 4)]
    public override void Example() => Run(695266907);

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from showr in Showr.ForAll(() => Please.AllowMe().ToReplace<int>(a => $"PREFIXED {a}"))
        from anInt in Checkr.Input("AnInt", Fuzzr.Int())
        from trace in Checkr.Trace("AnInt", () => anInt)
        from fail in Checkr.Expect("not 7", () => anInt != 7)
        select Case.Closed;

    protected override void Verify(Article article)
    {
        Assert.Equal("not 7", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(1, article.Total().Traces());
        Assert.Equal(0, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("AnInt", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("7", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("PREFIXED 7", article.Execution(1).Input(1).Read().Original.Value);
        Assert.Equal("AnInt", article.Execution(1).Trace(1).Read().Label);
        Assert.Equal("PREFIXED 7", article.Execution(1).Trace(1).Read().Value);
    }
}
