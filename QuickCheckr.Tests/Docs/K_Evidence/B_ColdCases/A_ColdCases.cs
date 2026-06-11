using QuickCheckr.Authoring.ThePress;
using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.FilingCabinet;
using QuickCheckr.Protocol;
using QuickCheckr.Tests.Drafts.B_PuttingACaseOnTheBoard.C_Configuration.B_TheCustodian;
using QuickCheckr.Tests.Tools;
using QuickCheckr.UnderTheHood;
using QuickCheckr.UnderTheHood.Archiving;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Docs.K_Evidence.B_ColdCases;


[DocFile]
public class A_ColdCases : BaseTest
{
    private static string Normalize(string path) => path.Replace('\\', '/').Replace("//", "/");

    [CodeExample]
    private static CheckrConfig Configure(CheckrConfig config)
        => config with { FileAs = "MyQuickCheckrTest" };

    [CodeSnippet]
    private static void RunColdCasesReview(CheckrOf<Case> checkr, Func<CheckrConfig, CheckrConfig> Configure) =>
        checkr.Configure(Configure).ReviewColdCases();

    [Fact]
    [DocHeader("Reviewing Cold Cases")]
    [DocContent(
@"Once an evidence vault exists the seeds therein contained can be reviewed.  
This is done by calling `ReviewColdCases` on the Checkr instance, passing in the same configuration used to create the evidence vault.")]
    [DocExample(typeof(A_ColdCases), nameof(RunColdCasesReview))]
    [DocContent("This will create a report per seed in the evidence vault.")]
    public void ReviewingColdCases()
    {
        var custodian = new InMemoryCustodian([new VaultEntry(1862095796, 1)]);

        Assert.Throws<FalsifiableException>(() =>
            RunColdCasesReview(Checkr.Expect("Fails", () => false),
                cfg => Configure(cfg with { Custodian = custodian })));

        Assert.Equal("MyQuickCheckrTest_1862095796", custodian.Records.Single(a => a.Value is CaseFile).Key);

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

        Assert.Throws<FalsifiableException>(() =>
            RunColdCasesReview(Checkr.Expect("Fails", () => false),
                cfg => Configure(cfg with { Custodian = custodian })));

        var caseFile = custodian.Records.Values.Single(a => a is CaseFile);
        var article = new Article(caseFile);

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

        var caseFile = custodian.Records.Values.Single(a => a is CaseFile);
        var article = new Article(caseFile);

        Assert.Equal("", article.FailureDescription());
        Assert.Equal(1, article.Total().PassedExpectations());
        Assert.Equal(0, article.ShrinkCount);
        Assert.Equal("Fails", article.PassedExpectation(1).Read().Label);
        Assert.Equal(1, article.PassedExpectation(1).Read().TimesPassed);
    }

    [Fact]
    [DocContent("- The main report contains a summary of the case files.")]
    public void ReviewingColdCases_Summary()
    {
        var custodian = new InMemoryCustodian([new VaultEntry(1, 1), new VaultEntry(2, 1)]); // 1 fails, 2 passes
        var checkr =
            from input in Checkr.Input("input", Fuzzr.Bool())
            from expect in Checkr.Expect("input is true", () => input)
            select Case.Closed;

        Assert.Throws<FalsifiableException>(() =>
            RunColdCasesReview(checkr,
                cfg => Configure(cfg with { Custodian = custodian })));

        var record = custodian.Records.Values.Single(a => a is ColdCasesReview);
        var article = new Article(record);

        // TODO TEST CONTENT

        WriteReport(article, "Example.txt", false);
    }

    [Fact]
    [DocContent("- `ReviewColdCases` throws a `FalsifiableException` if any previous seed still fails.")]
    public void ReviewingColdCases_ThrowsIfAtleastOneFailed()
    {
        var custodian = new InMemoryCustodian([new VaultEntry(1, 1), new VaultEntry(2, 1)]); // 1 fails, 2 passes
        var checkr =
            from input in Checkr.Input("input", Fuzzr.Bool())
            from expect in Checkr.Expect("input is true", () => input)
            select Case.Closed;

        Assert.Throws<FalsifiableException>(
            () => RunColdCasesReview(checkr,
            cfg => Configure(cfg with { Custodian = custodian })));
    }

    [Fact]
    [DocContent("- `ReviewColdCases` requires `FileAs`. If this is not configured it throws a `VaultNotConfiguredException`.")]
    public void ReviewingColdCases_ThrowsIfFileAsIsNotConfigured()
    {
        Assert.Throws<VaultNotConfiguredException>(
            () => Checkr.Expect("Fails", () => false).Configure(a => a).ReviewColdCases());
    }

    [Fact]
    [DocHeader("Closing Cold Cases")]
    [DocContent(
@"The method mentioned above is usefull for regression testing, but sometimes,
there is no need to keep the evidence around after the culprit is found.  
In this case we use:")]
    [DocExample(typeof(A_ColdCases), nameof(RunCloseResolvedColdCases))]
    [DocContent(
@"If the seeds stored in the vault still all result in failure, the behaviour is exactly the same as `ReviewColdCases`.")]
    public void ClosingColdCases()
    {
        var custodian = new InMemoryCustodian([new VaultEntry(1862095796, 1)]);

        Assert.Throws<FalsifiableException>(() =>
            RunCloseResolvedColdCases(Checkr.Expect("Fails", () => false),
                cfg => Configure(cfg with { Custodian = custodian })));

        Assert.Equal("MyQuickCheckrTest_1862095796", custodian.Records.Single(a => a.Value is CaseFile).Key);

        Assert.Single(custodian.Entries);
        var entry = custodian.Entries.Single();
        Assert.Equal(1862095796, entry.Seed);
        Assert.Equal(1, entry.ExecutionsPerRun);

        var caseFile = custodian.Records.Values.Single(a => a is CaseFile);
        var article = new Article(caseFile);

        Assert.Equal("Fails", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(0, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
    }

    [CodeSnippet]
    private static void RunCloseResolvedColdCases(CheckrOf<Case> checkr, Func<CheckrConfig, CheckrConfig> Configure) =>
        checkr.Configure(Configure).CloseResolvedColdCases();

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

        Assert.Empty(custodian.Entries);
        Assert.Empty(custodian.Records.Where(a => a.Value is CaseFile));
        Assert.Equal("MyQuickCheckrTest_1862095796", custodian.RemoveFileCalledWith);
    }

    [Fact]
    [DocContent("- The main report contains a summary of the case files.")]
    public void ClosingColdCases_Summary()
    {
        var custodian = new InMemoryCustodian([new VaultEntry(1, 1), new VaultEntry(2, 1)]); // 1 fails, 2 passes
        var checkr =
            from input in Checkr.Input("input", Fuzzr.Bool())
            from expect in Checkr.Expect("input is true", () => input)
            select Case.Closed;

        Assert.Throws<FalsifiableException>(() => RunCloseResolvedColdCases(checkr,
            cfg => Configure(cfg with { Custodian = custodian })));

        var record = custodian.Records.Values.Single(a => a is ColdCasesReview);
        var article = new Article(record);

        // TODO TEST CONTENT

        WriteReport(article, "Example_Closing.txt", true);
    }

    [Fact]
    [DocContent("- `CloseResolvedColdCases` throws a `FalsifiableException` if any previous seed still fails.")]
    public void ClosingColdCases_ThrowsIfAtleastOneFailed()
    {
        var custodian = new InMemoryCustodian([new VaultEntry(1, 1), new VaultEntry(2, 1)]); // 1 fails, 2 passes
        var checkr =
            from input in Checkr.Input("input", Fuzzr.Bool())
            from expect in Checkr.Expect("input is true", () => input)
            select Case.Closed;

        Assert.Throws<FalsifiableException>(
            () => RunCloseResolvedColdCases(checkr,
            cfg => Configure(cfg with { Custodian = custodian })));
    }

    [Fact]
    [DocContent("- `CloseResolvedColdCases` requires`FileAs`. If this is not configured it throws a `VaultNotConfiguredException`.")]
    public void ClosingColdCases_ThrowsIfFileAsIsNotConfigured()
    {
        Assert.Throws<VaultNotConfiguredException>(
            () => Checkr.Expect("Fails", () => false).Configure(a => a).CloseResolvedColdCases());
    }
}