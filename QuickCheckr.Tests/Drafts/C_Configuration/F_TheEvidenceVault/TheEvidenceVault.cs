using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress;
using QuickCheckr.UnderTheHood.Protocol;
using QuickPulse.Arteries;
using QuickPulse.Explains;
using QuickPulse.Explains.Text;


namespace QuickCheckr.Tests.Drafts.C_Configuration.F_TheEvidenceVault;

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
        Func<string, IArtery>? artery = null,
        IEnumerable<string>? entries = null)
        => cfg with
        {
            Custodian = new Custodian
            {
                Recorder = fn =>
                {
                    collector.Add(fn);
                    return artery == null ? Install.Null : artery(fn);
                },
                Retriever = fn => entries ?? [],
            }
        };

    [CodeSnippet]
    [CodeRemove("@\"")]
    [CodeRemove("\";")]
    private static string DefaultFileName()
        => @".quickcheckr\MyQuickCheckrTest\report.qr";


    [Fact]
    [DocContent("QuickCheckr can be configured to persist the results of a failed test run.")]
    [DocExample(typeof(TheEvidenceVault), nameof(Configure))]
    [DocContent("Above configuration will write the resulting report of a failing run to a `report.qr` file in a folder relative to the solution root:")]
    [DocExample(typeof(TheEvidenceVault), nameof(DefaultFileName))]
    public void ReportLocation()
    {
        var collector = new List<string>();
        TheJournalist.Exposes(() =>
            Checkr.Spec("Fails", () => false).Run(cfg => Configure(AddCustodian(cfg, collector))));
        Assert.Equal(3, collector.Count);
        Assert.Equal(Normalize(DefaultFileName()), Normalize(collector[0]));
    }

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

    private static Func<string, IArtery> SpyArteries(
        Collector<string> evidenceCollector,
        Collector<string> logCollector,
        int? seed = null,
        StringSink? reportCollector = null) =>
        fn =>
        {
            var normalized = Normalize(fn);
            if (normalized == Normalize(DefaultEvidenceFileName())) return evidenceCollector;
            if (normalized == Normalize(DefaultEvidenceLogFileName())) return logCollector;
            if (normalized == Normalize($".quickcheckr/MyQuickCheckrTest/vault/report_{seed}.qr"))
                return reportCollector ?? Install.Null;
            return Install.Null;
        };

    [Fact]
    [DocContent("In addition a sub folder is created containing the evidence vault files:")]
    [DocExample(typeof(TheEvidenceVault), nameof(DefaultEvidenceFileName))]
    [DocExample(typeof(TheEvidenceVault), nameof(DefaultEvidenceLogFileName))]
    [DocContent(
@"
- `evidence.qv` contains a list of all failing seeds, whenever a test fails, the seed is added to this file.
- `evidence-log.ql` is an append only log of all evidence vault actions.")]
    public void EvidenceLocation()
    {
        var collector = new List<string>();
        var evidence = Collect.ValuesOf<string>();
        var log = Collect.ValuesOf<string>();
        TheJournalist.Exposes(() =>
            Checkr.Spec("Fails", () => false).Run(1862095796, cfg =>
                Configure(AddCustodian(
                    cfg,
                    collector,
                    SpyArteries(evidence, log)))));
        Assert.Equal(3, collector.Count);
        Assert.Equal(Normalize(DefaultEvidenceFileName()), Normalize(collector[1]));
        Assert.Single(evidence.Values);
        Assert.Equal("1862095796,1,Fails", evidence.Values[0]);
        Assert.Equal(Normalize(DefaultEvidenceLogFileName()), Normalize(collector[2]));
        Assert.Single(log.Values);
        Assert.Equal($"Added Seed: 1862095796.{Environment.NewLine}", log.Values[0]);
    }

    [CodeSnippet]
    private static void RunColdCasesReview(CheckrOf<Case> checkr, Func<CheckrConfig, CheckrConfig> Configure) =>
        checkr.ReviewColdCases(Configure);

    [Fact]
    [DocHeader("Reviewing the Cold Cases")]
    [DocContent(
@"Once an evidence vault exists the seeds therein contained can be reviewed.  
This is done by calling `ReviewColdCases` on the Checkr instance, passing in the same configuration used to create the evidence vault.")]
    [DocExample(typeof(TheEvidenceVault), nameof(RunColdCasesReview))]
    [DocContent(
@"It is advised to *Dry* out the configuration as is shown at the top of this chapter, in order to avoid code drift.  
This will create a report per seed in the evidence file and a log of all evidence vault actions.
")]
    public void ReviewingColdCases()
    {
        var collector = new List<string>();
        var evidence = Collect.ValuesOf<string>();
        var log = Collect.ValuesOf<string>();

        RunColdCasesReview(Checkr.Spec("Fails", () => false),
            cfg => Configure(AddCustodian(
                    cfg,
                    collector,
                    SpyArteries(evidence, log),
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
        var evidence = Collect.ValuesOf<string>();
        var log = Collect.ValuesOf<string>();
        var report = Text.Capture();

        RunColdCasesReview(Checkr.Spec("Fails", () => false),
            cfg => Configure(AddCustodian(
                    cfg,
                    collector,
                    SpyArteries(evidence, log, 1862095796, report),
                    ["1862095796,1,Fails"])));
        Assert.Single(log.Values);
        Assert.Equal($"Run failed for Seed: 1862095796.{Environment.NewLine}", log.Values[0]);
        var reader = LinesReader.FromText(report.Content());
        Assert.Equal(" ------------------------------------------------------------", reader.NextLine());
        Assert.Equal(" Original failing run:    1 execution", reader.NextLine());
        Assert.Equal(" Seed:                    1862095796", reader.NextLine());
        Assert.Equal(" ------------------------------------------------------------", reader.NextLine());
        Assert.Equal("  Executed", reader.NextLine());
        Assert.Equal(" =========================", reader.NextLine());
        Assert.Equal("  !! Spec Failed: Fails", reader.NextLine());
        Assert.Equal(" =========================", reader.NextLine());
        Assert.True(reader.EndOfContent());
    }

    [Fact]
    [DocContent("- For every passing seed the report will only contain a list of passed specs.")]
    public void ReviewingColdCases_SeedPasses()
    {
        var collector = new List<string>();
        var evidence = Collect.ValuesOf<string>();
        var log = Collect.ValuesOf<string>();
        var report = Text.Capture();

        RunColdCasesReview(Checkr.Spec("Fails", () => true),
            cfg => Configure(AddCustodian(
                    cfg,
                    collector,
                    SpyArteries(evidence, log, 1862095796, report),
                    ["1862095796,1,Fails"])));
        Assert.Single(log.Values);
        Assert.Equal($"Run passed for Seed: 1862095796.{Environment.NewLine}", log.Values[0]);
        var reader = LinesReader.FromText(report.Content());
        Assert.Equal(" Passed Specs", reader.NextLine());
        Assert.Equal(" - Fails: 1x", reader.NextLine());
        Assert.Equal(" ------------------------------------------------------------", reader.NextLine());
        Assert.True(reader.EndOfContent());
    }
}