using QuickCheckr;
using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Tests.Tools;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Drafts.K_Evidence.A_StoringCaseFiles;


[DocFileHeader("When the Checkr passes.")]
public class A_PassedRun : CheckrTest<A_PassedRun>
{
    protected override bool Asserts => false;
    protected override bool Report => false;
    protected override bool Explain => false;

    [Fact]
    [DocContent("QuickCheckr provides the following *findings* about the run:")]
    [DocCheckr]
    [DocReport]
    public override void Example()
        => Document(a => a.Run(5.Runs(), 10.ExecutionsPerRun()));

    private const string FileName = @".quickcheckr\MyQuickCheckrTest\report.qr";

    protected override void Verify(Article article)
    {
        Assert.Equal("", article.FailureDescription());
        Assert.Equal("", article.VerifyFailed());
        Assert.Equal(1, article.Total().PassedExpectations());
        Assert.Equal("Always Passes", article.PassedExpectation(1).Read().Label);
        Assert.Equal(50, article.PassedExpectation(1).Read().TimesPassed);
    }

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr()
        => Checkr.Expect("Always Passes", () => true);
}