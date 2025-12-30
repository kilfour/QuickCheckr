using QuickCheckr.Protocol;
using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;
using QuickPulse.Show;


namespace QuickCheckr.Tests.Drafts.D_CheckrsCourt.C_TheShowrClass;

[DocFile]
public class TheShowrClass : BaseTest
{
    [Fact]
    [DocExample(typeof(TheShowrClass), nameof(GetCheckr))]
    [DocCodeFile("TheShowrClass.txt", "text")]
    public void RunTest()
    {
        var article = TheJournalist.Exposes(() =>
            GetCheckr().Run(502142190, 2.ExecutionsPerRun()));
        WriteAsserts(article, false);
        WriteReport<TheShowrClass>(article, true);
        Verify(article);
    }

    [CodeSnippet]
    [CodeRemove("return")]
    private static CheckrOf<Case> GetCheckr()
    {
        return
            from acc in Trackr.Stashed(() => new Accumelator())
            from count in Checkr.Input("Counter", Fuzzr.Counter("count"))
            from show in Showr.ForInputs(() => Please.AllowMe().ToReplace<int>(a => $"Int {a}"))
            from a in Checkr.Input("a", Fuzzr.Int())
            from b in Checkr.Input("b", Fuzzr.Int())
            from add in Checkr.Act("Add", () => acc.Add(a, b))
            from notAnswerOnTwo in Checkr.Expect("not 42 on execution 2", () => !(count == 2 && acc.Total == 42))
            select Case.Closed;
    }

    private static void Verify(Article article)
    {

    }

    [CodeExample]
    public class Accumelator
    {
        private int total;
        public int Total => total;
        public void Add(params int[] numbers)
            => total += numbers.Sum();
    }
}