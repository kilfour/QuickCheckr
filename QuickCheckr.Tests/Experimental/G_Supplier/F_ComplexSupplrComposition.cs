using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Supplier;
using QuickCheckr.Tests.Tools;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Experimental.G_Supplier;

[DocFile]
public class F_ComplexSupplrComposition : CheckrTest<F_ComplexSupplrComposition>
{
    protected override bool Report => false;

    [Fact]
    [DocCheckrHeader]
    [DocCheckr]
    public override void Example() =>
        Document(configured => configured.Run(100.ExecutionsPerRun()));

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from composed in Checkr.Input<Composed>("Composed",
            from aProp in Supplr<Composed>.Property(a => a.A, Supplr.Int(10, 20))
            from bProp in Supplr<Composed>.Property(a => a.B, Supplr.Int(20, 30))
                // from bProp in Supplr<Composed>.Property(a => a.B, Supplr.Int(aProp or aProp.?(), 30))
            select Intent.Fixed)
        from expectA in Checkr.Expect("A is inside its supplied range",
            () => composed.A >= 10 && composed.A < 20)
        from expectB in Checkr.Expect("B is inside its supplied range",
            () => composed.B >= composed.A)
        select Case.Closed;

    protected override void Verify(Article article)
    {
        Assert.Equal(string.Empty, article.FailureDescription());
        Assert.Equal(100, article.PassedExpectation(1).Read().TimesPassed);
        Assert.Equal(100, article.PassedExpectation(2).Read().TimesPassed);
    }

    public class Composed
    {
        public int A { get; set; }
        public int B { get; set; }
    }
}
