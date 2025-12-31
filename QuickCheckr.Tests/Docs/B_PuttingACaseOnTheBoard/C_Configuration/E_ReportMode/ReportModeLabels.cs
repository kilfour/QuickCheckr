using QuickCheckr.Protocol;
using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickPulse.Explains;


namespace QuickCheckr.Tests.Docs.B_PuttingACaseOnTheBoard.C_Configuration.E_ReportMode;

[DocFileHeader("ReportMode.Labels")]
public class ReportModeLabels : CheckrTest<ReportModeLabels>
{
    protected override bool Asserts => true;
    protected override bool Report => true;
    protected override bool Explanation => false;

    [Fact]
    [DocConfigure]
    [DocContent("Only traces.")]
    [DocReport]
    public override void Example()
        => Run(1964562926, 2.ExecutionsPerRun(), Configure);

    [CodeExample]
    protected override CheckrConfig Configure(CheckrConfig config)
        => config with { ReportMode = ReportMode.Labels };

    protected override CheckrOf<Case> TheCheckr()
        => ReportModeTests.GetCheckr();

    protected override void Verify(Article article)
    {

    }
}
