using QuickCheckr.Authoring.ThePress;
using QuickCheckr.Protocol;
using QuickCheckr.Tests.Drafts.B_PuttingACaseOnTheBoard.C_Configuration;
using QuickCheckr.Tests.Drafts.B_PuttingACaseOnTheBoard.C_Configuration.B_TheCustodian;
using QuickCheckr.Tests.Drafts.B_PuttingACaseOnTheBoard.E_GatheringEvidence;
using QuickCheckr.Tests.Tools;
using QuickCheckr.UnderTheHood.Archiving;
using QuickPulse.Explains;


namespace QuickCheckr.Tests.Drafts.B_PuttingACaseOnTheBoard.D_TheEvidenceVault;

[DocFile]
[DocLink(typeof(PuttingACaseOnTheBoard))]
[DocLink(typeof(Configuration))]
[DocLink(typeof(GatheringEvidence))]
[DocContent("[The Board][PuttingACaseOnTheBoard] | [Previous][Configuration] | [Next][GatheringEvidence]  \n")]
public class TheEvidenceVault : BaseTest
{
    private static string Normalize(string path) => path.Replace('\\', '/').Replace("//", "/");

    [CodeExample]
    private static CheckrConfig Configure(CheckrConfig config)
        => config with { FileAs = "MyQuickCheckrTest" };

    private static string DefaultEvidenceFileName()
        => @".quickcheckr\MyQuickCheckrTest\vault\evidence.qv";

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
    [DocContent("This will create a report per seed in the evidence vault.")]
    public void ReviewingColdCases()
    {
        var custodian = new InMemoryCustodian([new VaultEntry(1862095796, 1)]);

        RunColdCasesReview(Checkr.Expect("Fails", () => false),
            cfg => Configure(cfg with { Custodian = custodian }));

        Assert.Equal(Normalize(DefaultEvidenceFileName()), Normalize(custodian.EvidenceFileName));
        Assert.Equal(Normalize(DefaultReportFileName(1862095796)), Normalize(custodian.Records.Keys.Single()));

        Assert.Single(custodian.Entries);
        var entry = custodian.Entries.Single();
        Assert.Equal(1862095796, entry.Seed);
        Assert.Equal(1, entry.ExecutionsPerRun);
    }

    [Fact]
    [DocContent("- For every failing seed the report will look like the by now familiar failure report.")]
    public void ReviewingColdCases_SeedStillFails()
    {
        var custodian = new InMemoryCustodian([new VaultEntry(1862095796, 1)]);

        RunColdCasesReview(Checkr.Expect("Fails", () => false),
            cfg => Configure(cfg with { Custodian = custodian }));

        var caseFile = custodian.Records.Values.Single();
        var article = TheJournalist.Unearths(caseFile);

        Assert.Equal("Fails", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(0, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
    }

    [Fact]
    [DocContent("- For every passing seed the report will only contain a list of passed expectations.")]
    public void ReviewingColdCases_SeedPasses()
    {
        var custodian = new InMemoryCustodian([new VaultEntry(1862095796, 1)]);

        RunColdCasesReview(Checkr.Expect("Fails", () => true),
            cfg => Configure(cfg with { Custodian = custodian }));

        var caseFile = custodian.Records.Values.Single();
        var article = TheJournalist.Unearths(caseFile);

        Assert.Equal("", article.FailureDescription());
        Assert.Equal(1, article.Total().PassedExpectations());
        Assert.Equal(0, article.ShrinkCount);
        Assert.Equal("Fails", article.PassedExpectation(1).Read().Label);
        Assert.Equal(1, article.PassedExpectation(1).Read().TimesPassed);
    }

    [Fact]
    [DocHeader("Closing Cold Cases")]
    [DocContent(
@"The method mentioned above is usefull for regression testing, but sometimes,
there is no need to keep the evidence around after the culprit is found.  
In this case we use:")]
    [DocExample(typeof(TheEvidenceVault), nameof(RunCloseResolvedColdCases))]
    [DocContent(
@"If the seeds stored in the vault still all result in failure, the behaviour is exactly the same as `ReviewColdCases`.")]
    public void ClosingColdCases()
    {
        var custodian = new InMemoryCustodian([new VaultEntry(1862095796, 1)]);

        RunCloseResolvedColdCases(Checkr.Expect("Fails", () => false),
            cfg => Configure(cfg with { Custodian = custodian }));

        Assert.Equal(Normalize(DefaultEvidenceFileName()), Normalize(custodian.EvidenceFileName));
        Assert.Equal(Normalize(DefaultReportFileName(1862095796)), Normalize(custodian.Records.Keys.Single()));

        Assert.Single(custodian.Entries);
        var entry = custodian.Entries.Single();
        Assert.Equal(1862095796, entry.Seed);
        Assert.Equal(1, entry.ExecutionsPerRun);

        var caseFile = custodian.Records.Values.Single();
        var article = TheJournalist.Unearths(caseFile);

        Assert.Equal("Fails", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(0, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
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
        var custodian = new InMemoryCustodian([new VaultEntry(1862095796, 1)]);

        RunCloseResolvedColdCases(Checkr.Expect("Fails", () => true),
            cfg => Configure(cfg with { Custodian = custodian }));

        Assert.Equal(Normalize(DefaultEvidenceFileName()), Normalize(custodian.EvidenceFileName));

        Assert.Empty(custodian.Entries);
        // FILE DELETED is untested as this is handled by specific Custodians

        Assert.Empty(custodian.Records);
        Assert.Equal(Normalize(DefaultReportFileName(1862095796)), Normalize(custodian.RemoveFileCalledWith));
    }
}