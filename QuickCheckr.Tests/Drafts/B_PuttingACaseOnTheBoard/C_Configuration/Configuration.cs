using QuickCheckr.Protocol;
using QuickCheckr.Tests.Drafts.B_PuttingACaseOnTheBoard.B_RunningACheckr;
using QuickCheckr.Tests.Drafts.B_PuttingACaseOnTheBoard.C_Configuration.B_TheCustodian;
using QuickCheckr.Tests.Drafts.B_PuttingACaseOnTheBoard.C_Configuration.J_ReportMode;
using QuickCheckr.Tests.Drafts.B_PuttingACaseOnTheBoard.C_Configuration.J_ReportMode.Reference;
using QuickCheckr.Tests.Drafts.B_PuttingACaseOnTheBoard.C_Configuration.N_ShrinkMode;
using QuickCheckr.Tests.Drafts.B_PuttingACaseOnTheBoard.C_Configuration.N_ShrinkMode.Reference;
using QuickCheckr.Tests.Drafts.B_PuttingACaseOnTheBoard.C_Configuration.P_Hint;
using QuickPulse.Explains;


namespace QuickCheckr.Tests.Drafts.B_PuttingACaseOnTheBoard.C_Configuration;

[DocFile]
[DocLink(typeof(PuttingACaseOnTheBoard))]
[DocLink(typeof(RunningACheckr))]
[DocLink(typeof(A_ReportModeReference))]
[DocLink(typeof(A_ShrinkModeReference))]
[DocContent("[The Board][PuttingACaseOnTheBoard] | [Previous][RunningACheckr] | [Next][TheEvidenceVault]  \n")]
[DocContent(
@"QuickCheckr runs are configured through an immutable `CheckrConfig` record. 
You typically start from `CheckrConfig.Default()` and then tweak the parts you care about.

All configuration options live in the `QuickCheckr.Protocol` namespace.")]
[DocContent("\n**Default Configuration:**")]
[DocExample(typeof(Configuration), nameof(DefaultSettings))]
[DocInclude(typeof(TheCustodian))]
[DocInclude(typeof(ReportModeTests))]
[DocInclude(typeof(ShrinkModeTests))]
[DocInclude(typeof(Hint))]
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
            WarningLevel = WarningLevel.Important,
            MaxStepsPerReducer = 1024
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
        // more ?
    }
}
