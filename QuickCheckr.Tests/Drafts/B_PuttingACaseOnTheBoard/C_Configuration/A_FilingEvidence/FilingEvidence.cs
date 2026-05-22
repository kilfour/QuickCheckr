using QuickCheckr.Authoring.ThePress;
using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Protocol;
using QuickCheckr.Tests.Drafts.B_PuttingACaseOnTheBoard.C_Configuration.B_TheCustodian;
using QuickCheckr.Tests.Tools;
using QuickPulse.Explains;


namespace QuickCheckr.Tests.Drafts.B_PuttingACaseOnTheBoard.C_Configuration.A_FilingEvidence;

//[DocFile]
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
    [DocContent("QuickCheckr can be configured to persist the results of a test run.")]
    [DocExample(typeof(FilingEvidence), nameof(Configure))]
    [DocContent("Above configuration will write the resulting report of a run to a `report.qr` file in a folder relative to the solution root:")]
    [DocExample(typeof(FilingEvidence), nameof(DefaultFileName))]
    public void ReportLocation()
    {
        var custodian = new InMemoryCustodian([]);
        TheJournalist.Exposes(() =>
            Checkr.Expect("Fails", () => false).Run(cfg =>
                Configure(cfg with { Custodian = custodian })));
        Assert.Equal(Normalize(DefaultFileName()), Normalize(custodian.Records.Keys.Single()));
    }

    [Fact]
    [DocContent("In case a test passes, a *summary report* is provided.")]
    [DocCodeFile("FilingEvidenceRunPassed.txt", "text")]
    public void RunPassedReport()
    {
        var custodian = new InMemoryCustodian([]);
        Checkr.Expect("Always Passes", () => true).Run(cfg =>
            Configure(cfg with { Custodian = custodian }));
        Assert.Equal(Normalize(DefaultFileName()), Normalize(custodian.Records.Keys.Single()));
        var caseFile = custodian.Records.Values.Single();
        var article = TheJournalist.Unearths(caseFile);
        WriteReport(article, "FilingEvidenceRunPassed.txt", false);
        VerifyPassedReport(article);
    }

    private static void VerifyPassedReport(Article article)
    {
        Assert.Equal(1, article.Total().PassedExpectations());
        Assert.Equal("Always Passes", article.PassedExpectation(1).Read().Label);
        Assert.Equal(1, article.PassedExpectation(1).Read().TimesPassed);
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

    This *evidence* can later be used to potentially solve *Cold Cases*.")]
    public void EvidenceLocation()
    {
        var custodian = new InMemoryCustodian([]);
        TheJournalist.Exposes(() =>
            Checkr.Expect("Fails", () => false).Run(1862095796, cfg =>
                Configure(cfg with { Custodian = custodian })));

        Assert.Equal(Normalize(DefaultEvidenceFileName()), Normalize(custodian.EvidenceFileName));

        Assert.Single(custodian.Entries);
        var entry = custodian.Entries.Single();
        Assert.Equal(1862095796, entry.Seed);
        Assert.Equal(1, entry.ExecutionsPerRun);
    }
}