using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Drafts.J_YouCanGoYourOwnWay.D_CustomShrinkers.CustomShrinkersIncludes;

[DocFileHeader("A Second Example")]
public class B_SecondShrinker : CheckrTest<B_SecondShrinker>
{
    protected override bool Asserts => false;
    protected override bool Report => false;

    [Fact]
    [DocCheckrHeader]
    [DocCheckr]
    [DocReportHeader]
    [DocReport]
    public override void Example() =>
        Run(960262083);

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from answers in Checkr.Input("answers", Fuzzr.Int().Many(1, 5),
            Shrink.OnType<int>(
                s => s.Classify(Try.Values(42)),
                s => s.Simplify(Reduce.Towards(100))))
        from trace in Checkr.Trace("original", () => answers)
        from expect in Checkr.Expect("all answers equal 42", () => answers.All(a => a == 42))
        select Case.Closed;

    protected override void Verify(Article article)
    {

    }
}
