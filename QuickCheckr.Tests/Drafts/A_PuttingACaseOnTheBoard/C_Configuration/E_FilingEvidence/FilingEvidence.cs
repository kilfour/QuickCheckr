using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress;
using QuickCheckr.UnderTheHood.Protocol;
using QuickPulse.Arteries;
using QuickPulse.Explains;


namespace QuickCheckr.Tests.Drafts.A_PuttingACaseOnTheBoard.C_Configuration.E_FilingEvidence;

[DocFile]
public class FilingEvidence : BaseTest
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
    [DocExample(typeof(FilingEvidence), nameof(Configure))]
    [DocContent("Above configuration will write the resulting report of a failing run to a `report.qr` file in a folder relative to the solution root:")]
    [DocExample(typeof(FilingEvidence), nameof(DefaultFileName))]
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
    [DocExample(typeof(FilingEvidence), nameof(DefaultEvidenceFileName))]
    [DocExample(typeof(FilingEvidence), nameof(DefaultEvidenceLogFileName))]
    [DocContent(
@"
- `evidence.qv` contains a list of all failing seeds, whenever a test fails, the seed is added to this file.
- `evidence-log.ql` is an append only log of all evidence vault actions.

This *evidence* can later be used to potentially solve *Cold Cases*.")]
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
}