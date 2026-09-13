using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Protocol;
using QuickCheckr.Tests.Tools;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Drafts.H_OnShrinking.B_ActionShrinking;

[DocFile]
[DocContent(
@"Action shrinking only evaluates actions created with `Checkr.ShrinkableAct`.

A regular `Checkr.Act` remains in the report and continues to execute during replay, preserving
both the action itself and any dependencies that pass through it.")]
public class B_RegularActsAreNotSkipped : CheckrTest<B_RegularActsAreNotSkipped>
{
    protected override bool Report => false;
    protected override bool Explain => false;

    [Fact]
    [DocCheckrHeader]
    [DocCheckr]
    [DocReportHeader]
    [DocReport]
    [DocContent(
@"Neither regular action is considered as a shrinking candidate, so the report retains both the
irrelevant first action and the second action that throws.")]
    public override void Example() =>
        Document(configured => configured
            .Configure(config => config with { ShrinkMode = ShrinkMode.Actions })
            .Run(593203923));

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from actOne in Checkr.Act("Act One", () => { })
        from actTwo in Checkr.Act("Act Two", () => throw new InvalidOperationException("BOOM"))
        select Case.Closed;

    protected override void Verify(Article article)
    {
        Assert.Equal("InvalidOperationException: BOOM", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(2, article.Total().Actions());
        Assert.Equal(0, article.ShrinkCount);
        Assert.Equal("Act One", article.Execution(1).Action(1).Read().Label);
        Assert.Equal("Act Two", article.Execution(1).Action(2).Read().Label);
    }
}
