using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Protocol;
using QuickCheckr.Tests.Tools;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Drafts.H_OnShrinking.B_ActionShrinking;

[DocFile]
[DocContent(
@"Mixing regular and shrinkable acts can hide a dependency from the minimal report.

Here `Prepare` is a regular act and therefore always returns `true`, even after its action label
has been removed. `Explode` is shrinkable: removing its label really skips its body and makes the
run pass, so it must be retained.")]
public class C_MixedActsHideRegularDependencies : CheckrTest<C_MixedActsHideRegularDependencies>
{
    protected override bool Report => true;
    protected override bool Explain => false;

    [Fact]
    [DocCheckrHeader]
    [DocCheckr]
    [DocReportHeader]
    [DocReport]
    [DocContent(
@"The report retains `Explode` but removes `Prepare`. This does not mean that the preparation was
unnecessary: its regular action body continued to run during the replay that justified removing
its label.")]
    public override void Example() =>
        Document(configured => configured
            .Configure(config => config with { ShrinkMode = ShrinkMode.Actions })
            .Run(1489110479));

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from prepared in Checkr.Act("Prepare", () => true)
        from explode in Checkr.ShrinkableAct("Explode", () =>
        {
            if (prepared)
                throw new InvalidOperationException("BOOM");
        })
        select Case.Closed;

    protected override void Verify(Article article)
    {
        Assert.Equal("InvalidOperationException: BOOM", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Actions());
        Assert.Equal(1, article.ShrinkCount);
        Assert.Equal("Explode", article.Execution(1).Action(1).Read().Label);
    }
}
