
using QuickCheckr.Authoring;
using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Protocol;
using QuickCheckr.Tests.Drafts.B_PuttingACaseOnTheBoard.C_Configuration.B_TheCustodian;
using QuickCheckr.Tests.Tools;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Docs.K_Evidence.A_StoringCaseFiles;


[DocFileHeader("When the Checkr passes.")]
public class A_PassedRun : CheckrTest<A_PassedRun>
{
    protected override bool Asserts => false;
    protected override bool Report => false;
    protected override bool Explain => false;

    [Fact]
    [DocContent("QuickCheckr provides the following *findings* about the run:")]
    [DocCheckrHeader]
    [DocCheckr]
    [DocReportHeader]
    [DocReport]
    public override void Example() => TheCheckr().Run(5.Runs(), 10.ExecutionsPerRun(), Configure);

    private const string FileName = @".quickcheckr\MyQuickCheckrTest\report.qr";

    private readonly InMemoryCustodian custodian = new([]);

    private CheckrConfig Configure(CheckrConfig config)
        => config with { FileAs = "MyQuickCheckrTest", Custodian = custodian };

    protected override void Verify(Article article)
    {
        Assert.Equal("", article.FailureDescription());
        Assert.Equal("", article.VerifyFailed());
        Assert.Equal(1, article.Total().PassedExpectations());
        Assert.Equal("Always Passes", article.PassedExpectation(1).Read().Label);
        Assert.Equal(50, article.PassedExpectation(1).Read().TimesPassed);
        // --
        // Check if the custodian received the findings
        Assert.Equal(Normalize.Path(FileName), Normalize.Path(custodian.Records.Keys.Single()));
        Assert.Equal(article.Record, custodian.Records.Values.Single());
    }

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr()
        => Checkr.Expect("Always Passes", () => true);
}