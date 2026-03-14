using QuickCheckr.Protocol;
using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Drafts.J_YouCanGoYourOwnWay.D_CustomShrinkers.CustomShrinkersIncludes;

[DocFile]
public class A_FirstShrinker : CheckrTest<A_FirstShrinker>
{
    protected override bool Asserts => true;
    protected override bool Report => true;
    protected override bool Explanation => false;

    [Fact]
    [DocCheckrHeader]
    [DocCheckr]
    [DocReportHeader]
    [DocReport]
    public override void Example() =>
        Run(1895340861);

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from answers in Checkr.Input("answer", Fuzzr.Int(),
            Shrink.OnType<int>(
                s => s.Classify(Try.Values(42)),
                s => s.Simplify(Reduce.Towards(100))))
        from trace in Checkr.Trace("original", () => answers)
        from expect in Checkr.Expect("all answers equal 42", () => answers == 42)
        select Case.Closed;

    protected override void Verify(Article article)
    {

    }
}
