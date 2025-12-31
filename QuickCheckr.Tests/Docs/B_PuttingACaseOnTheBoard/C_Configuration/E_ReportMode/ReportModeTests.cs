using QuickCheckr.Protocol;
using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;
using QuickPulse.Instruments;

namespace QuickCheckr.Tests.Docs.B_PuttingACaseOnTheBoard.C_Configuration.E_ReportMode;


[DocFileHeader("ReportMode")]
[DocContent(
@"Controls whether or not *Traces*, *Warnings*  and/or *Exception StackTraces* show up in the report.

The following Checkr will be used to generate the reports throughout this chapter:")]
[DocExample(typeof(ReportModeTests), nameof(GetCheckr))]
[DocInclude(typeof(ReportModeDefault))]
[DocInclude(typeof(ReportModeNone))]
[DocInclude(typeof(ReportModeTrace))]
[DocInclude(typeof(ReportModeWarning))]
[DocInclude(typeof(ReportModeLabels))]
[DocContent("**Hint: You can use bit-wise operations to manipulate ReportMode``:**")]
[DocExample(typeof(ReportModeTests), nameof(BitWiseOperationExample))]
public class ReportModeTests : BaseTest
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
            from trace in Checkr.Trace("Irrelevant", () => irrelevant)
            from boom in Checkr.Act("Blows up on 2", () => counter != 2 || ComputerSays.No<bool>("Only one is available"))
            select Case.Closed;
    }

    [CodeSnippet]
    private static CheckrConfig BitWiseOperationExample(CheckrConfig config) =>
        config with { ReportMode = config.ReportMode & ~ReportMode.StackTrace };
}