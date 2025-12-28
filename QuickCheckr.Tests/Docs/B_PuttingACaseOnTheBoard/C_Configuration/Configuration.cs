using QuickCheckr.Protocol;
using QuickCheckr.Tests.Docs.B_PuttingACaseOnTheBoard.B_RunningACheckr;
using QuickCheckr.Tests.Docs.B_PuttingACaseOnTheBoard.C_Configuration.A_FilingEvidence;
using QuickCheckr.Tests.Docs.B_PuttingACaseOnTheBoard.C_Configuration.B_TheCustodian;
using QuickCheckr.Tests.Docs.B_PuttingACaseOnTheBoard.C_Configuration.D_ShrinkMode;
using QuickCheckr.Tests.Docs.B_PuttingACaseOnTheBoard.C_Configuration.E_ReportMode;
using QuickCheckr.Tests.Docs.B_PuttingACaseOnTheBoard.C_Configuration.F_DeliberationPolicy;
using QuickCheckr.Tests.Docs.B_PuttingACaseOnTheBoard.C_Configuration.H_InternalAffairs;
using QuickCheckr.Tests.Docs.B_PuttingACaseOnTheBoard.D_TheEvidenceVault;
using QuickPulse.Explains;


namespace QuickCheckr.Tests.Docs.B_PuttingACaseOnTheBoard.C_Configuration;

[DocFile]
[DocLink(typeof(PuttingACaseOnTheBoard))]
[DocLink(typeof(RunningACheckr))]
[DocLink(typeof(TheEvidenceVault))]
[DocContent("[Up][PuttingACaseOnTheBoard] | [Previous][RunningACheckr] | [Next][TheEvidenceVault]  \n")]
[DocContent(
@"QuickCheckr runs are configured through an immutable `CheckrConfig` record. 
You typically start from `CheckrConfig.Default()` and then tweak the parts you care about.

All configuration options live in the `QuickCheckr.Protocol` namespace.")]
[DocContent("\n**Default Configuration:**")]
[DocExample(typeof(Configuration), nameof(DefaultSettings))]
[DocInclude(typeof(FilingEvidence))]
[DocInclude(typeof(TheCustodian))]
[DocInclude(typeof(ShrinkModeTests))]
[DocInclude(typeof(ReportModeTests))]
[DocInclude(typeof(DeliberationPolicyTests))]
[DocInclude(typeof(InternalAffairsTests))]
[DocContent("\n[Up][PuttingACaseOnTheBoard] | [Previous][RunningACheckr] | [Next][TheEvidenceVault]")]
public class Configuration
{
    [CodeSnippet]
    [CodeRemove("return new CheckrConfig()")]
    [CodeRemove(";")]
    private static CheckrConfig DefaultSettings()
    {
        return new CheckrConfig()
        {
            FileAs = string.Empty,
            Custodian = Custodian.Default,
            ShrinkMode = ShrinkMode.Default,
            ReportMode = ReportMode.Default,
            DeliberationPolicy = null,
            InternalAffairs = []
        };
    }

    [Fact]

    public void DefaultConfig()
    {
        var expected = DefaultSettings();
        var actual = CheckrConfig.Default();
        Assert.Equal(expected.FileAs, actual.FileAs);
        // Assert below touches the file system
        // Assert.IsType<Custodian.FileRecorder>(actual.Custodian.Recorder("test"));
        Assert.Equal(expected.ShrinkMode, actual.ShrinkMode);
        Assert.Equal(expected.ReportMode, actual.ReportMode);
        Assert.Equal(expected.DeliberationPolicy, actual.DeliberationPolicy);


        Assert.Equal(expected.InternalAffairs, actual.InternalAffairs);
        // more ?
    }
}