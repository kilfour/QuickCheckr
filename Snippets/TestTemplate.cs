using QuickCheckr;
using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress;
using QuickCheckr.Authoring.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;


namespace QuickCheckr.Tests;

[DocFile]
public class TestTemplate : BaseTest
{
    [Fact]
    [DocExample(typeof(TestTemplate), nameof(GetCheckr))]
    [DocCodeFile("TestTemplate.txt", "text")]
    public void RunTest()
    {
        var article = TheJournalist.Exposes(() =>
            GetCheckr().Run(100.Runs(), 1.ExecutionsPerRun()));
        WriteAsserts(article, false);
        WriteReport<TestTemplate>(article, false);
        Verify(article);
    }

    [CodeSnippet]
    [CodeRemove("return")]
    private static CheckrOf<Case> GetCheckr()
    {
        return
            from act in Checkr.Act("Act", () => { })
            select Case.Closed;
    }

    private static void Verify(Article article)
    {

    }
}