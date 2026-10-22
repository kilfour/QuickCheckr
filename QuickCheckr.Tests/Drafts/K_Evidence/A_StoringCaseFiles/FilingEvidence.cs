using QuickCheckr.Protocol;
using QuickCheckr.Tests.Docs.B_PuttingACaseOnTheBoard.C_Configuration.B_TheCustodian;
using QuickCheckr.Tests.Tools;
using QuickCheckr.UnderTheHood;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Drafts.K_Evidence.A_StoringCaseFiles;


[DocFile]
public class FilingEvidence : BaseTest
{
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
        Assert.Throws<FalsifiableException>(
            () => Checkr.Expect("Fails", () => false).Configure(a => Configure(a with { Custodian = custodian })).Run());
        Assert.Equal("MyQuickCheckrTest", custodian.Records.Keys.Single());
    }


    [CodeSnippet]
    [CodeRemove("@\"")]
    [CodeRemove("\";")]
    private static string DefaultEvidenceFileName()
        => @".quickcheckr\MyQuickCheckrTest\vault\evidence.qv";

    [Fact]
    [DocContent("In addition a sub folder is created containing the evidence vault files:")]
    [DocExample(typeof(FilingEvidence), nameof(DefaultEvidenceFileName))]
    [DocContent(
"""
`evidence.qv` contains a list of all failing seeds, whenever a test fails, the seed is added to this file.

This *evidence* can later be used to potentially solve *Cold Cases*.
""")]
    [DocInclude(typeof(A_PassedRun))]
    public void EvidenceLocation()
    {
        var custodian = new InMemoryCustodian([]);
        Assert.Throws<FalsifiableException>(
            () => Checkr.Expect("Fails", () => false)
                .Configure(a => Configure(a with { Custodian = custodian }))
                .Run(1862095796));

        Assert.Single(custodian.Entries);
        var entry = custodian.Entries.Single();
        Assert.Equal(1862095796, entry.Seed);
        Assert.Equal(1, entry.ExecutionsPerRun);
    }
}