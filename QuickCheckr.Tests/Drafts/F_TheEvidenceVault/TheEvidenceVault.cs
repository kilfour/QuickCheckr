using QuickCheckr.Tests.Tools;
using QuickCheckr.UnderTheHood.Protocol;
using QuickPulse.Explains;
using QuickPulse.Explains.Text;


namespace QuickCheckr.Tests.Drafts.F_TheEvidenceVault;

[DocFile]
public class TheEvidenceVault : BaseTest
{
    private static string Normalize(string path) => path.Replace('\\', '/').Replace("//", "/");

    [CodeExample]
    private static CheckrConfig Configure(CheckrConfig config)
        => config with { FileAs = "MyQuickCheckrTest" };

    private static CheckrConfig AddCustodian(
        CheckrConfig cfg,
        List<string> collector,
        Func<string, IEvidenceRecorder> recorder,
        IEnumerable<string>? entries = null)
        => cfg with
        {
            Custodian = new Custodian
            {
                Recorder = fn =>
                {
                    collector.Add(fn);
                    return recorder(fn);
                },
                Retriever = fn => entries ?? [],
            }
        };

    [CodeSnippet]
    [CodeRemove("@\"")]
    [CodeRemove("\";")]
    private static string DefaultEvidenceFileName()
        => @".quickcheckr\MyQuickCheckrTest\vault\evidence.qv";

    [CodeSnippet]
    [CodeRemove("@\"")]
    [CodeRemove("\";")]
    private static string DefaultEvidenceLogFileName()
        => @".quickcheckr\MyQuickCheckrTest\vault\evidence-log.ql";

    private static Func<string, IEvidenceRecorder> SpyArteries(
        SpyRecorder evidenceCollector,
        SpyRecorder logCollector,
        int? seed,
        SpyRecorder reportCollector) =>
        fn =>
        {
            var normalized = Normalize(fn);
            if (normalized == Normalize(DefaultEvidenceFileName())) return evidenceCollector;
            if (normalized == Normalize(DefaultEvidenceLogFileName())) return logCollector;
            if (normalized == Normalize($".quickcheckr/MyQuickCheckrTest/vault/report_{seed}.qr"))
                return reportCollector;
            throw new Exception($"Unexpected artifact: {fn}");
        };

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
        var collector = new List<string>();
        var evidence = new SpyRecorder();
        var log = new SpyRecorder();

        RunColdCasesReview(Checkr.Expect("Fails", () => false),
            cfg => Configure(AddCustodian(
                    cfg,
                    collector,
                    SpyArteries(evidence, log, 1862095796, new SpyRecorder()),
                    ["1862095796,1,Fails"])));

        Assert.Equal(3, collector.Count);
        Assert.Equal(Normalize(".quickcheckr/MyQuickCheckrTest/vault/evidence.qv"), Normalize(collector[0]));
        Assert.Equal(Normalize(".quickcheckr/MyQuickCheckrTest/vault/evidence-log.ql"), Normalize(collector[1]));
        Assert.Equal(Normalize(".quickcheckr/MyQuickCheckrTest/vault/report_1862095796.qr"), Normalize(collector[2]));
        Assert.Single(evidence.Values);
        Assert.Equal("1862095796,1,Fails", evidence.Values[0]);
    }

    [Fact]
    [DocContent("- For every failing seed the report will look like the by now familiar failure report.")]
    public void ReviewingColdCases_SeedStillFails()
    {
        var collector = new List<string>();
        var evidence = new SpyRecorder();
        var log = new SpyRecorder();
        var report = new SpyRecorder();

        RunColdCasesReview(Checkr.Expect("Fails", () => false),
            cfg => Configure(AddCustodian(
                    cfg,
                    collector,
                    SpyArteries(evidence, log, 1862095796, report),
                    ["1862095796,1,Fails"])));
        Assert.Single(log.Values);
        Assert.Equal($"Run failed for Seed: 1862095796.", log.Values[0]);
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
        var collector = new List<string>();
        var evidence = new SpyRecorder();
        var log = new SpyRecorder();
        var report = new SpyRecorder();

        RunColdCasesReview(Checkr.Expect("Fails", () => true),
            cfg => Configure(AddCustodian(
                    cfg,
                    collector,
                    SpyArteries(evidence, log, 1862095796, report),
                    ["1862095796,1,Fails"])));
        Assert.Single(log.Values);
        Assert.Equal($"Run passed for Seed: 1862095796.", log.Values[0]);
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
        var collector = new List<string>();
        var evidence = new SpyRecorder();
        var log = new SpyRecorder();

        RunCloseResolvedColdCases(Checkr.Expect("Fails", () => false),
            cfg => Configure(AddCustodian(
                    cfg,
                    collector,
                    SpyArteries(evidence, log, 1862095796, new SpyRecorder()),
                    ["1862095796,1,Fails"])));

        Assert.Equal(3, collector.Count);
        Assert.Equal(Normalize(".quickcheckr/MyQuickCheckrTest/vault/evidence.qv"), Normalize(collector[0]));
        Assert.Equal(Normalize(".quickcheckr/MyQuickCheckrTest/vault/evidence-log.ql"), Normalize(collector[1]));
        Assert.Equal(Normalize(".quickcheckr/MyQuickCheckrTest/vault/report_1862095796.qr"), Normalize(collector[2]));
        Assert.Single(evidence.Values);
        Assert.Equal("1862095796,1,Fails", evidence.Values[0]);
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
        var collector = new List<string>();
        var evidence = new SpyRecorder();
        var log = new SpyRecorder();
        var report = new SpyRecorder();

        RunCloseResolvedColdCases(Checkr.Expect("Passes", () => true),
            cfg => Configure(AddCustodian(
                    cfg,
                    collector,
                    SpyArteries(evidence, log, 1862095796, report),
                    ["1862095796,1,Fails"])));

        Assert.Equal(3, collector.Count);
        Assert.Equal(Normalize(".quickcheckr/MyQuickCheckrTest/vault/evidence.qv"), Normalize(collector[0]));
        Assert.Equal(Normalize(".quickcheckr/MyQuickCheckrTest/vault/evidence-log.ql"), Normalize(collector[1]));
        Assert.Equal(Normalize(".quickcheckr/MyQuickCheckrTest/vault/report_1862095796.qr"), Normalize(collector[2]));
        Assert.Empty(evidence.Values);
        Assert.True(evidence.Cleared);
        Assert.True(report.Cleared);
    }
}