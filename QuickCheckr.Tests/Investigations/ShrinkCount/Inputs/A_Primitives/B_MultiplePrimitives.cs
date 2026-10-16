using QuickCheckr;
using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Tests.Tools;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Investigations.ShrinkCount.Inputs.A_Primitives;

[DocFile]
public class B_MultiplePrimitives : CheckrTest<B_MultiplePrimitives>
{
    protected override bool Asserts => false;
    protected override bool Report => false;

    [Fact]
    [DocCheckr]
    [DocReport]
    public override void Example() => Document(a => a.Run(1874338076));

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from input in Checkr.Input("Answer", Fuzzr.Constant(42))
        from irrelevant in Checkr.Input("Irrelevant", Fuzzr.Int())
        from irrelevantAgain in Checkr.Input("Irrelevant Again", Fuzzr.String())
        from expect in Checkr.Expect("Fails", () => input != 42)
        select Case.Closed;

    protected override void Verify(Article article)
    {
        Assert.Equal(2, article.ShrinkCount);
    }
}