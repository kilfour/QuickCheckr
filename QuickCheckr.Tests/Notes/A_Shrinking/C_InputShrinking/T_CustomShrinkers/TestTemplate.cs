using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;


namespace QuickCheckr.Tests.Notes.A_Shrinking.C_InputShrinking.T_CustomShrinkers;

[DocFile]
public class TestTemplate : BaseTest
{
    //[Fact]
    public void RunTest()
    {
        var article = TheJournalist.Exposes(() =>
            GetCheckr().Run(100.Runs(), 100.ExecutionsPerRun(), a => a with { Seed = null }));
        WriteAsserts(article, true);
        WriteReport<TestTemplate>(article, true);
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
            from act in Checkr.Act("Act", () => { })
            select Case.Closed;
    }
}