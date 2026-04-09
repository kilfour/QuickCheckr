using QuickCheckr.Protocol;
using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Investigations.G_GatheringEvidence;

[DocFile]
public class A_ReportsRedux : CheckrTest<A_ReportsRedux>
{
    protected override bool Asserts => true;
    protected override bool Report => true;
    protected override bool Explain => false;

    [Fact]
    [DocCheckrHeader]
    [DocCheckr]
    [DocReportHeader]
    [DocReport]
    public override void Example() =>
        Run(1883509239);

    protected override CheckrConfig Configure(CheckrConfig config)
        => config with { ReportMode = config.ReportMode | ReportMode.FinalTrace };

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from input in Checkr.Input("Input", Fuzzr.Int(), Shrink.OnType<int>(s => s.Simplify(Reduce.TowardsZero)))
        from trace in Checkr.Trace("Input", () => input)
        from expect in Checkr.Expect("<= 42", () => input <= 42)
        select Case.Closed;

    protected override void Verify(Article article)
    {
    }
}
