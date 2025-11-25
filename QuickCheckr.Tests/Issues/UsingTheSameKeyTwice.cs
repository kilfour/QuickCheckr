using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;


namespace QuickCheckr.Tests.Reference;

[DocFile]
public class UsingTheSameKeyTwice : BaseTest
{
    [Fact]
    [DocExample(typeof(UsingTheSameKeyTwice), nameof(GetCheckr))]
    [DocCodeFile("TestTemplate.txt", "text")]
    public void RunTest()
    {
        var article = TheJournalist.Unearths(
            GetCheckr().Run(100.Runs(), 100.ExecutionsPerRun(), a => a with { Seed = null }));
        WriteAsserts(article, true);
        WriteReport<UsingTheSameKeyTwice>(article, true);
        Verify(article);
    }

    private static void Verify(Article article)
    {

    }

    [CodeSnippet]
    [CodeRemove("return")]
    private static CheckrOf<Case> GetCheckr()
    {
        return
            from i in Checkr.Input("reused", Fuzzr.Int(100, 200))
            from counter in Checkr.Input("reused", Fuzzr.Counter("counter"))
            from spec in Checkr.Spec("Counter does not equal 3", () => counter != 3)
            select Case.Closed;
    }
}