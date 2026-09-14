using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Protocol;
using QuickCheckr.Tests.Tools;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Drafts.H_OnShrinking.B_ActionShrinking;

[DocFile]
[DocContent(
@"Regular and shrinkable acts can be mixed without hiding dependencies from the minimal report.

Here `Prepare` is a regular act and is not considered for removal. `Explode` is shrinkable:
removing its label skips its body and makes the run pass, so it must also be retained.")]
public class C_MixedActsHideRegularDependencies : CheckrTest<C_MixedActsHideRegularDependencies>
{
    protected override bool Report => false;
    protected override bool Explain => false;

    [Fact]
    [DocCheckr]
    [DocReport]
    [DocContent(
@"The report retains both `Prepare` and `Explode`. Only `Explode` is evaluated as a shrinking
candidate; the ordinary action remains visible and available to its dependent action.")]
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
        Assert.Equal(2, article.Total().Actions());
        Assert.Equal(0, article.ShrinkCount);
        Assert.Equal("Explode", article.Execution(1).Action(1).Read().Label);
        Assert.Equal("Prepare", article.Execution(1).Action(2).Read().Label);
    }
}
