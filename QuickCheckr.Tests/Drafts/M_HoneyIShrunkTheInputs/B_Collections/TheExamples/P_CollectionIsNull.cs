using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Protocol;
using QuickCheckr.Tests.Tools;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Drafts.M_HoneyIShrunkTheInputs.B_Collections.TheExamples;

[DocFile]
public class P_CollectionIsNull : CheckrTest<P_CollectionIsNull>
{
    protected override bool Asserts => false;
    protected override bool Report => false;

    [Fact]
    [DocConfigureHeader]
    [DocConfigure]
    [DocCheckrHeader]
    [DocCheckr]
    [DocReportHeader]
    [DocReport]
    public override void Example()
        => Run(849984174);

    [CodeSnippet]
    protected override CheckrConfig Configure(CheckrConfig config)
        => config with { WarningLevel = WarningLevel.Verbose };

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from list in Checkr.Input("List", Fuzzr.Constant<IEnumerable<int>>(null!))
        from expect in Checkr.Expect("Always fails.", () => false)
        select Case.Closed;

    protected override void Verify(Article article)
    {
        Assert.Equal("Always fails.", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(1, article.Total().Warnings());
        Assert.Equal(0, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("List", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("null", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("Null value for type 'IEnumerable`1' at input 'List'.", article.Execution(1).Warning(1).Read().Value);
    }
}
