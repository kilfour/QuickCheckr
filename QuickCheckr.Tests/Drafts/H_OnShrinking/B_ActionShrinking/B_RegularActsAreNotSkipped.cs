using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Protocol;
using QuickCheckr.Tests.Tools;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Drafts.H_OnShrinking.B_ActionShrinking;

[DocFile]
[DocContent(
@"Action shrinking can only skip actions created with `Checkr.ShrinkableAct`.

A regular `Checkr.Act` still records an action label during the original run, so the action
shrinker will consider that label. During replay, however, a regular act does not consult the
remaining labels and its body always executes. Consequently its label can be removed even when
the action itself causes the failure.")]
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
@"Both action labels disappear and count as shrinks, but `Act Two` continues to execute and
throw during every replay. The resulting report therefore contains the exception without the
action that produced it.")]
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
        Assert.Equal(0, article.Total().Actions());
        Assert.Equal(2, article.ShrinkCount);
    }
}
