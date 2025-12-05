using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickCheckr.UnderTheHood.Protocol;
using QuickPulse.Arteries;
using QuickPulse.Explains;
using QuickPulse.Show;


namespace QuickCheckr.Tests.Drafts.F_TheEvidenceVault;

[DocFile]

public class TheEvidenceVault : BaseTest
{
    [CodeExample]
    private static CheckrConfig Configure(CheckrConfig config)
        => config with { FileAs = "MyQuickCheckrTest" };

    private static CheckrConfig AddCustodian(CheckrConfig cfg, IArtery artery, List<string> collector)
        => cfg with
        {
            Custodian = new Custodian
            {
                Recorder = fn => { collector.Add(fn); return artery; },
                Retriever = fn => [],
            }
        };

    [CodeSnippet]
    [CodeRemove("@\"")]
    [CodeRemove("\";")]
    private static string DefaultFileName()
        => @".quickcheckr\MyQuickCheckrTest\report.qr";

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
    [DocContent("QuickCheckr can be configured to persist the results of a failed test run.")]
    [DocExample(typeof(TheEvidenceVault), nameof(Configure))]
    [DocContent("Above configuration will write the resulting report of a failing run to a `report.qr` file in a folder relative to the solution root:")]
    [DocExample(typeof(TheEvidenceVault), nameof(DefaultFileName))]
    //[DocCodeFile("TheEvidenceVault.txt", "text")]
    public void ReportLocation()
    {
        var collector = new List<string>();
        var stringSink = Collect.ValuesOf<string>();
        TheJournalist.Exposes(() =>
            Checkr.Spec("Fails", () => false).Run(cfg => Configure(AddCustodian(cfg, stringSink, collector))));
        //stringSink.Values.PulseToLog();
        Assert.Equal(3, collector.Count);
        Assert.Equal(DefaultFileName(), collector[0]);
        Assert.Equal(DefaultEvidenceFileName(), collector[1]);
        Assert.Equal(DefaultEvidenceLogFileName(), collector[2]);
        ExplainMe<TheEvidenceVault>(true);
    }

    [Fact]
    //[DocCodeFile("TheEvidenceVault.txt", "text")]
    public void RunTest()
    {
        //GetCheckr().ReviewColdCases(Configure);
        //var collector = new List<string>();
        var article = TheJournalist.Exposes(() =>
            Checkr.Spec("Fails", () => false).Run(Configure));
        //Assert.Single(collector);
        //Assert.Equal(".quickcheckr\\MyQuickCheckrTest\\report.qr", collector[0]);
        // WriteAsserts(article, false);
        // WriteReport<TheEvidenceVault>(article, false);
        // Verify(article);
        //ExplainMe<TheEvidenceVault>(true);
    }

    private static void Verify(Article article)
    {

    }
}