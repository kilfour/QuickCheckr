using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Supplier;
using QuickCheckr.Tests.Tools;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Experimental.G_Supplier;

[DocFile]
[DocContent(
    "A Supplr describes a valid domain and can be passed directly to `CheckrX.Input`.")]
public class A_IntegerRange : CheckrTest<A_IntegerRange>
{
    protected override bool Report => false;

    [Fact]
    [DocCheckr]
    public override void Example() =>
        Document(configured => configured.Run(100.ExecutionsPerRun()));

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from age in CheckrX.Input("Age", Supplr.Int(18, 80))
        from expect in Checkr.Expect(
            "Age is inside its supplied range",
            () => age >= 18 && age < 80)
        select Case.Closed;

    protected override void Verify(Article article)
    {
        Assert.Equal(string.Empty, article.FailureDescription());
        Assert.Equal(100, article.PassedExpectation(1).Read().TimesPassed);
    }
}
