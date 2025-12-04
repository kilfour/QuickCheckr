using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickCheckr.UnderTheHood.Protocol;
using QuickPulse.Explains;


namespace QuickCheckr.Tests.Drafts.F_TheEvidenceVault;

[DocFile]
[DocContent(
@"QuickCheckr can be configured to persist the results of a failed test run.")]
[DocExample(typeof(TheEvidenceVault), nameof(Configure))]
public class TheEvidenceVault : BaseTest
{

    [Fact]
    [DocContent(
@"Above configuration will create a `report.qr` file in a folder relative to the solution root named `.quickcheckr/MyQuickCheckrTest/`.  ")]
    public void ReportLocation()
    {

    }

    [Fact]
    [DocExample(typeof(TheEvidenceVault), nameof(GetCheckr))]
    [DocCodeFile("TheEvidenceVault.txt", "text")]
    public void RunTest()
    {
        GetCheckr().ReviewColdCases(Configure);
        var article = TheJournalist.Exposes(() =>
            GetCheckr().Run(100.Runs(), 1.ExecutionsPerRun(), Configure));
        // WriteAsserts(article, false);
        // WriteReport<TheEvidenceVault>(article, false);
        // Verify(article);
    }

    [CodeExample]
    private static CheckrConfig Configure(CheckrConfig config)
        => config with { FileAs = "MyQuickCheckrTest" };


    private static void Verify(Article article)
    {

    }

    [CodeSnippet]
    [CodeRemove("return")]
    private static CheckrOf<Case> GetCheckr()
    {
        return
            from spec in Checkr.Spec("Fails", () => false)
            select Case.Closed;
    }
}