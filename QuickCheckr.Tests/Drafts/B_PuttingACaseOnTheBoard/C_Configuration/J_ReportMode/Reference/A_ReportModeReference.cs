using QuickCheckr.Protocol;
using QuickCheckr.Tests.Tools;
using QuickFuzzr;
using QuickPulse.Explains;
using QuickPulse.Instruments;

namespace QuickCheckr.Tests.Drafts.B_PuttingACaseOnTheBoard.C_Configuration.J_ReportMode.Reference;

[DocFile]
[DocFileHeader("ReportMode Reference")]
[DocContent("The following Checkr will be used to generate the reports throughout this chapter:")]
[DocExample(typeof(A_ReportModeReference), nameof(GetCheckr))]
[DocInclude(typeof(ReportModeDefault))]
[DocInclude(typeof(ReportModeNone))]
[DocInclude(typeof(ReportModeTrace))]
[DocInclude(typeof(ReportModeFinalTrace))]
[DocContent("**Hint:** You can use bit-wise operations to manipulate `ReportMode`:")]
[DocExample(typeof(A_ReportModeReference), nameof(BitWiseOperationExample))]
public class A_ReportModeReference : BaseTest
{
    public class Thing;

    [CodeSnippet]
    [CodeRemove("return")]
    public static CheckrOf<Case> GetCheckr()
    {
        return
            from counter in Checkr.Input("Counter", Fuzzr.Counter("counter"))
            from irrelevant in Checkr.Input("Irrelevant", Fuzzr.Int())
            from nullThing in Checkr.Input("Null Thing", Fuzzr.Constant((Thing)null!))
            from trace in Checkr.Trace("Irrelevant", a => a.Show(irrelevant))
            from boom in Checkr.Act("Blows up on 2", () => counter != 2 || ComputerSays.No<bool>("Only one is available"))
            select Case.Closed;
    }

    [CodeSnippet]
    private static CheckrConfig BitWiseOperationExample(CheckrConfig config) =>
        config with { ReportMode = config.ReportMode & ~ReportMode.Trace };
}