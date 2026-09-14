using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Supplier;
using QuickCheckr.Tests.Tools;
using QuickCheckr.UnderTheHood;
using QuickPulse.Explains;
using QuickPulse.Show;

namespace QuickCheckr.Tests.Experimental.G_Supplier;

[DocFile]
[DocContent(
    "TODO.")]
public class G_ComplexSupplrCompositionClassification :
    CheckrTest<G_ComplexSupplrCompositionClassification>
{
    protected override bool Report => false;
    protected override bool Asserts => false;

    [Fact]
    [DocCheckr]
    public override void Example() =>
        Document(configured => configured.Run(1656426057));

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from composed in CheckrX.Input("Composed", ComposedSupplr())
        from irrelevant in CheckrX.Input("Irrelevant", ComposedSupplr())
        from expect in Checkr.Expect("Fails", () => composed.A == 10)
        select Case.Closed;

    protected override void Verify(Article article)
    {
        Assert.Equal("Fails", article.FailureDescription());
        Assert.Equal("", article.VerifyFailed());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(0, article.Total().Inputs());
        Assert.Equal(0, article.ShrinkCount);
    }

    [Fact]
    public void ClassificationRespectsSupplr()
    {
        var composedCollector = new List<(int A, int B)>();
        var irrelevantCollector = new List<(int A, int B)>();
        var checkr =
            from composed in CheckrX.Input("Composed", ComposedSupplr())
            from irrelevant in CheckrX.Input("Irrelevant", ComposedSupplr())
            from perform in Checkr.Perform(() =>
            {
                composedCollector.Add((composed.A, composed.B));
                irrelevantCollector.Add((irrelevant.A, irrelevant.B));
            })
            from expect in Checkr.Expect("Fails", () => composed.A == 0)
            select Case.Closed;

        Assert.Throws<FalsifiableException>(() => checkr.Run());
        Assert.True(
            composedCollector.All(IsInsideSuppliedRanges),
            Introduce.This(composedCollector));
        Assert.True(
            irrelevantCollector.All(IsInsideSuppliedRanges),
            Introduce.This(irrelevantCollector));
    }

    private static bool IsInsideSuppliedRanges((int A, int B) value) =>
        value.A >= 10 && value.A < 20 &&
        value.B >= value.A && value.B < 30;

    private static SupplrOf<Composed> ComposedSupplr() =>
        from a in Supplr.Int(10, 20)
        from b in Supplr.Int(a, 30)
        select new Composed { A = a, B = b };

    public class Composed
    {
        public int A { get; set; }
        public int B { get; set; }
    }
}
