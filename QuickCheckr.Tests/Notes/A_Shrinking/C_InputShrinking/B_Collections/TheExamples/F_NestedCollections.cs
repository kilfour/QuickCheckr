using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;


namespace QuickCheckr.Tests.Notes.A_Shrinking.C_InputShrinking.B_Collections.TheExamples;

[DocFile]
public class F_NestedCollections : BaseTest
{
    [Fact]
    [DocExample(typeof(F_NestedCollections), nameof(GetCheckr))]
    [DocCodeFile("F_NestedCollections.txt", "text")]
    public void RunTest()
    {
        var article = TheJournalist.Exposes(() =>
            GetCheckr().Run(a => a with { Seed = 960809284 }));
        // WriteAsserts(article, true);
        // WriteReport<F_NestedCollections>(article, true);
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
            from input in Checkr.Input("List", Fuzzr.Int().Many(0, 10).Many(0, 10))
            from spec in Checkr.SpecIf("List[0][0] is not 42",
                () => input.Any() && input.First().Any(),
                () => input.First().First() != 42)
            select Case.Closed;
    }
}