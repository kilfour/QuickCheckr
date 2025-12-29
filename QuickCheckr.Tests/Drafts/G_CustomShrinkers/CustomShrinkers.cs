using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;


namespace QuickCheckr.Tests.Drafts.G_CustomShrinkers;

[DocFile]
[DocExample(typeof(CustomShrinkers), nameof(GetCheckr))]
[DocCodeFile("CustomShrinkers.txt", "text")]
public class CustomShrinkers : BaseTest
{
    [Fact]
    public void RunTest()
    {
        var article = TheJournalist.Exposes(() =>
            GetCheckr().Run());
        WriteAsserts(article);
        WriteReport<CustomShrinkers>(article, true);
        Verify(article);
    }

    [CodeSnippet]
    [CodeRemove("return")]
    private static CheckrOf<Case> GetCheckr()
    {
        return

            from thingOne in Checkr.Input("Thing One", Fuzzr.Constant<Thing>(null!))
            from shrinkThing in Shrinkr.For(Fuzzr.One<Thing>())
            from thingTwo in Checkr.Input("Thing Two", Fuzzr.Constant<Thing>(null!))
            from expect in Checkr.Expect("NotNull", () => thingTwo is not null)
            select Case.Closed;
    }

    private static void Verify(Article article)
    {

    }

    public class Thing;
}