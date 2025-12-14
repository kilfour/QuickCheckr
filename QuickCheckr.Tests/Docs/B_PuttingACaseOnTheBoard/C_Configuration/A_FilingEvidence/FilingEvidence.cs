using QuickCheckr.Protocol;
using QuickCheckr.Tests.Docs.B_PuttingACaseOnTheBoard.C_Configuration.B_TheCustodian;
using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress;
using QuickPulse.Explains;


namespace QuickCheckr.Tests.Docs.B_PuttingACaseOnTheBoard.C_Configuration.A_FilingEvidence;

[DocFile]
public class FilingEvidence : BaseTest
{
    private static string Normalize(string path) => path.Replace('\\', '/').Replace("//", "/");

    [CodeExample]
    private static CheckrConfig Configure(CheckrConfig config)
        => config with { FileAs = "MyQuickCheckrTest" };

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
        var custodian = new InMemoryCustodian();
        TheJournalist.Exposes(() =>
            Checkr.Expect("Fails", () => false).Run(cfg =>
                Configure(cfg with { Custodian = custodian.Build() })));
        Assert.Equal(3, custodian.Data.Count);
        Assert.Equal(Normalize(DefaultFileName()), Normalize(custodian.Data.Keys.ToList()[0]));
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
        var custodian = new InMemoryCustodian();
        TheJournalist.Exposes(() =>
            Checkr.Expect("Fails", () => false).Run(1862095796, cfg =>
                Configure(cfg with { Custodian = custodian.Build() })));
        Assert.Equal(3, custodian.Data.Count);
        Assert.Equal(Normalize(DefaultEvidenceFileName()), Normalize(custodian.Data.Keys.ToList()[1]));
        var evidence = custodian.Data.Values.ToList()[1];
        Assert.Single(evidence.Values);
        Assert.Equal("1862095796,1,Fails", evidence.Values[0]);
        Assert.Equal(Normalize(DefaultEvidenceLogFileName()), Normalize(custodian.Data.Keys.ToList()[2]));
        var log = custodian.Data.Values.ToList()[2];
        Assert.Single(log.Values);
        Assert.Equal($"Added Seed: 1862095796.", log.Values[0]);
    }
}