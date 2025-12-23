using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;


namespace QuickCheckr.Tests.Notes.T_Examples.W_Warehouse;

//[DocFile]
public class Warehouse : BaseTest
{
    [Fact(Skip = "WIP")]
    [DocExample(typeof(Warehouse), nameof(GetCheckr))]
    [DocCodeFile("Warehouse.txt", "text")]
    public void RunTest()
    {
        var article = TheJournalist.Exposes(() =>
            GetCheckr().Run(100.Runs(), 1.ExecutionsPerRun()));
        WriteAsserts(article, true);
        WriteReport<Warehouse>(article, true);
        ExplainMe<Warehouse>(false);
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