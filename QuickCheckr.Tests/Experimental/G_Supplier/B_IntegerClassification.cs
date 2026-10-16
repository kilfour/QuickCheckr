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
public class B_IntegerClassification : CheckrTest<B_IntegerClassification>
{
    protected override bool Report => false;
    protected override bool Asserts => false;

    [Fact]
    [DocCheckr]
    public override void Example() =>
        Document(configured => configured.Run(1656426057));

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from age in CheckrX.Input("Age", Supplr.Int(18, 80))
        from irrelevant in CheckrX.Input("Irrelevant", Supplr.Int(18, 80))
        from expect in Checkr.Expect("Fails", () => age == 20)
        select Case.Closed;

    protected override void Verify(Article article)
    {
        Assert.Equal("Fails", article.FailureDescription());
        Assert.Equal("", article.VerifyFailed());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(1, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Age", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("79", article.Execution(1).Input(1).Read().Value);
    }

    [Fact]
    public void ClassificationRespectsSupplr()
    {
        var ageCollector = new List<int>();
        var irrelevantCollector = new List<int>();
        var checkr =
            from age in CheckrX.Input("Age", Supplr.Int(18, 80))
            from irrelevant in CheckrX.Input("Irrelevant", Supplr.Int(18, 80))
            from perform in Checkr.Perform(() => { ageCollector.Add(age); irrelevantCollector.Add(irrelevant); })
            from expect in Checkr.Expect("Fails", () => age == 0)
            select Case.Closed;

        Assert.Throws<FalsifiableException>(() => checkr.Run());
        Assert.True(ageCollector.All(a => a >= 18 && a < 80), Introduce.This(ageCollector));
        Assert.True(irrelevantCollector.All(a => a >= 18 && a < 80), Introduce.This(irrelevantCollector));
    }
}
