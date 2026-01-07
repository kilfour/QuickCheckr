using QuickCheckr.Protocol;
using QuickCheckr.Tests.Tools;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Docs.B_PuttingACaseOnTheBoard.C_Configuration.J_ReportMode;


[DocFileHeader("ReportMode")]
[DocContent(
@"You can use this to customize report formatting somewhat.  
I personally almost always disable the *Labels* ('Input:', 'Pool:', etc...), but your milage may vary.  

It is also useful to toggle traces on or off, for instance.")]
[DocExample(typeof(ReportModeTests), nameof(DisableTraces))]
[DocContent("A full reference to the available options can be found [here][A_ReportModeReference].")]
public class ReportModeTests : BaseTest
{
    [CodeExample]
    private static CheckrConfig DisableTraces(CheckrConfig config) =>
        config with { ReportMode = config.ReportMode & ~ReportMode.Trace };
}