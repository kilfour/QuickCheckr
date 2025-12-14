using QuickCheckr.Protocol;
using QuickCheckr.Tests.Docs.B_PuttingACaseOnTheBoard.C_Configuration.B_TheCustodian;
using QuickCheckr.Tests.Tools;
using QuickPulse.Explains;
using QuickPulse.Explains.Text;


namespace QuickCheckr.Tests.Docs.B_PuttingACaseOnTheBoard.D_TheEvidenceVault;

[DocFile]
public class TheEvidenceVault : BaseTest
{
    private static string Normalize(string path) => path.Replace('\\', '/').Replace("//", "/");

    [CodeExample]
    private static CheckrConfig Configure(CheckrConfig config)
        => config with { FileAs = "MyQuickCheckrTest" };

    private static string DefaultEvidenceFileName()
        => @".quickcheckr\MyQuickCheckrTest\vault\evidence.qv";

    private static string DefaultEvidenceLogFileName()
        => @".quickcheckr\MyQuickCheckrTest\vault\evidence-log.ql";

    private static string DefaultReportFileName(int seed)
        => @$".quickcheckr\MyQuickCheckrTest\vault\report_{seed}.qr";

    [CodeSnippet]
    private static void RunColdCasesReview(CheckrOf<Case> checkr, Func<CheckrConfig, CheckrConfig> Configure) =>
        checkr.ReviewColdCases(Configure);

    [Fact]
    [DocHeader("Reviewing Cold Cases")]
    [DocContent(
@"Once an evidence vault exists the seeds therein contained can be reviewed.  
This is done by calling `ReviewColdCases` on the Checkr instance, passing in the same configuration used to create the evidence vault.")]
    [DocExample(typeof(TheEvidenceVault), nameof(RunColdCasesReview))]
    [DocContent("This will create a report per seed in the evidence file and a log of all evidence vault actions.")]
    public void ReviewingColdCases()
    {
        var custodian = new InMemoryCustodian { StartingEvidence = ["1862095796,1,Fails"] };

        RunColdCasesReview(Checkr.Expect("Fails", () => false),
            cfg => Configure(cfg with { Custodian = custodian.Build() }));

        Assert.Equal(3, custodian.Data.Count);
        Assert.Equal(Normalize(DefaultEvidenceFileName()), Normalize(custodian.Data.Keys.ToList()[0]));
        Assert.Equal(Normalize(DefaultEvidenceLogFileName()), Normalize(custodian.Data.Keys.ToList()[1]));
        Assert.Equal(Normalize(DefaultReportFileName(1862095796)), Normalize(custodian.Data.Keys.ToList()[2]));

        var evidence = custodian.Data.Values.ToList()[0];
        Assert.Single(evidence.Values);
        Assert.Equal("1862095796,1,Fails", evidence.Values[0]);

        var log = custodian.Data.Values.ToList()[1];
        Assert.Single(log.Values);
        Assert.Equal("Run failed for Seed: 1862095796.", log.Values[0]);
    }

    [Fact]
    [DocContent("- For every failing seed the report will look like the by now familiar failure report.")]
    public void ReviewingColdCases_SeedStillFails()
    {
        var custodian = new InMemoryCustodian { StartingEvidence = ["1862095796,1,Fails"] };

        RunColdCasesReview(Checkr.Expect("Fails", () => false),
            cfg => Configure(cfg with { Custodian = custodian.Build() }));

        var log = custodian.Data.Values.ToList()[1];
        var report = custodian.Data.Values.ToList()[2];

        Assert.Single(log.Values);
        Assert.Equal("Run failed for Seed: 1862095796.", log.Values[0]);

        var reader = LinesReader.FromText(report.Values[0]);
        Assert.Equal(" ------------------------------------------------------------", reader.NextLine());
        Assert.Equal(" Original failing run:    1 execution", reader.NextLine());
        Assert.Equal(" Seed:                    1862095796", reader.NextLine());
        Assert.Equal(" ------------------------------------------------------------", reader.NextLine());
        Assert.Equal("  Executed", reader.NextLine());
        Assert.Equal(" ================================", reader.NextLine());
        Assert.Equal("  !! Expectation Failed: Fails", reader.NextLine());
        Assert.Equal(" ================================", reader.NextLine());
        Assert.True(reader.EndOfContent());
    }

    [Fact]
    [DocContent("- For every passing seed the report will only contain a list of passed specs.")]
    public void ReviewingColdCases_SeedPasses()
    {
        var custodian = new InMemoryCustodian { StartingEvidence = ["1862095796,1,Fails"] };

        RunColdCasesReview(Checkr.Expect("Fails", () => true),
            cfg => Configure(cfg with { Custodian = custodian.Build() }));

        var log = custodian.Data.Values.ToList()[1];
        var report = custodian.Data.Values.ToList()[2];

        Assert.Single(log.Values);
        Assert.Equal("Run passed for Seed: 1862095796.", log.Values[0]);

        var reader = LinesReader.FromText(report.Values[0]);
        Assert.Equal(" Passed Expectations", reader.NextLine());
        Assert.Equal(" - Fails: 1x", reader.NextLine());
        Assert.Equal(" ------------------------------------------------------------", reader.NextLine());
        Assert.True(reader.EndOfContent());
    }

    [Fact]
    [DocHeader("Closing Cold Cases")]
    [DocContent(
@"The method mentioned above is usefull for regression testing, but sometimes,
    there is no need to keep the evidence around after the culprit is found.  
    In this case we use:")]
    [DocExample(typeof(TheEvidenceVault), nameof(RunCloseResolvedColdCases))]
    [DocContent(
@"In case that the seeds stored in the vault still result in failures, the behaviour is exactly the same as `ReviewColdCases`.")]
    public void ClosingColdCases()
    {
        var custodian = new InMemoryCustodian { StartingEvidence = ["1862095796,1,Fails"] };

        RunCloseResolvedColdCases(Checkr.Expect("Fails", () => false),
            cfg => Configure(cfg with { Custodian = custodian.Build() }));

        Assert.Equal(3, custodian.Data.Count);
        Assert.Equal(Normalize(DefaultEvidenceFileName()), Normalize(custodian.Data.Keys.ToList()[0]));
        Assert.Equal(Normalize(DefaultEvidenceLogFileName()), Normalize(custodian.Data.Keys.ToList()[1]));
        Assert.Equal(Normalize(DefaultReportFileName(1862095796)), Normalize(custodian.Data.Keys.ToList()[2]));

        var evidence = custodian.Data.Values.ToList()[0];
        Assert.Single(evidence.Values);
        Assert.Equal("1862095796,1,Fails", evidence.Values[0]);

        var log = custodian.Data.Values.ToList()[1];
        Assert.Single(log.Values);
        Assert.Equal("Run failed for Seed: 1862095796.", log.Values[0]);
    }

    [CodeSnippet]
    private static void RunCloseResolvedColdCases(CheckrOf<Case> checkr, Func<CheckrConfig, CheckrConfig> Configure) =>
        checkr.CloseResolvedColdCases(Configure);

    [Fact]
    [DocContent(
@"If however a run for a given seed no longer fails, the seed is removed from the evidence vault,
    and no report is generated for the passing run.  
    Any existing previous reports for runs that now pass are deleted, and if the evidence vault is empty, it is also deleted.")]
    public void ClosingColdCasesNowPass()
    {
        var custodian = new InMemoryCustodian { StartingEvidence = ["1862095796,1,Fails"] };

        RunCloseResolvedColdCases(Checkr.Expect("Fails", () => true),
            cfg => Configure(cfg with { Custodian = custodian.Build() }));

        Assert.Equal(3, custodian.Data.Count);
        Assert.Equal(Normalize(DefaultEvidenceFileName()), Normalize(custodian.Data.Keys.ToList()[0]));
        Assert.Equal(Normalize(DefaultEvidenceLogFileName()), Normalize(custodian.Data.Keys.ToList()[1]));
        Assert.Equal(Normalize(DefaultReportFileName(1862095796)), Normalize(custodian.Data.Keys.ToList()[2]));

        var evidence = custodian.Data.Values.ToList()[0];
        Assert.Empty(evidence.Values);
        Assert.True(evidence.Cleared);

        var log = custodian.Data.Values.ToList()[1];
        Assert.Equal(2, log.Values.Count);
        Assert.Equal("Run passed for Seed: 1862095796.", log.Values[0]);
        Assert.Equal("Removed Seed: 1862095796.", log.Values[1]);

        var report = custodian.Data.Values.ToList()[2];
        Assert.Empty(report.Values);
        Assert.True(report.Cleared);
    }
}