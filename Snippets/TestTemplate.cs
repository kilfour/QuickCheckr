using QuickCheckr;
using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;


namespace QuickCheckr.Tests;

[DocFile]
public class TestTemplate : BaseTest
{
    [CodeSnippet]
    [CodeRemove("return")]
    private static CheckrOf<Case> GetCheckr()
    {
        return
            from act in Checkr.Act("Act", () => { })
            select Case.Closed;
    }

    [Fact]
    [DocExample(typeof(TestTemplate), nameof(GetCheckr))]
    [DocCodeFile("TestTemplate.txt", "text")]
    public void RunTest()
    {
        var article = TheJournalist.Exposes(() =>
            GetCheckr().Run(100.Runs(), 1.ExecutionsPerRun(), a => a with { Seed = null }));
        WriteAsserts(article, true);
        WriteReport<TestTemplate>(article, true);
        ExplainMe<TestTemplate>(true);
        Verify(article);
    }

    private static void Verify(Article article)
    {

    }
}