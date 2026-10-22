using QuickCheckr.Shrinking;
using QuickCheckr;
using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Tests.Tools;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Investigations.ShrinkCount.Inputs.C_Objects;

[DocFile]
public class A_SingleObject : CheckrTest<A_SingleObject>
{
    protected override bool Asserts => false;
    protected override bool Report => false;

    [Fact]
    [DocCheckr]
    [DocReport]
    public override void Example() => Document(a => a.Run(1105886268));

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from input in Checkr.Input("Answer", Fuzzr.Constant(42))
        from irrelevant in Checkr.Input("Irrelevant", Fuzzr.One<Thing>(), Shrinkr.For<Thing>().Classify(Try.Values((Thing)null!)))
        from expect in Checkr.Expect("Fails", () => input != 42)
        select Case.Closed;

    protected override void Verify(Article article)
    {
        Assert.Equal(1, article.ShrinkCount);
    }

    public class Thing;
}
