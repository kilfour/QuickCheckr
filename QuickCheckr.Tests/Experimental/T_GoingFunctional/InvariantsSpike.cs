using QuickCheckr.Invariants;
using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Experimental.T_GoingFunctional;

[DocFile]
public class InvariantsSpike : BaseTest
{
    public class DocBoldHeaderAttribute(string header) :
        DocContentAttribute($"\n**{header}:**");

    [CodeExample]
    public class TheReverser
    {
        public static List<int> Reverses(List<int> list)
            => [.. HideTheAnswer(list.AsEnumerable()).Reverse()];

        private static IEnumerable<int> HideTheAnswer(IEnumerable<int> list)
            => list.Where(a => a != 42).Concat(list.Where(a => a == 42));
    }

    [Fact]
    [DocBoldHeader("The SUT")]
    [DocExample(typeof(TheReverser))]
    [DocBoldHeader("The Test")]
    [DocExample(typeof(InvariantsSpike), nameof(TheTest))]
    [DocBoldHeader("The Report")]
    [DocCodeFile("InvariantsSpike.txt", "text")]
    public void Go()
    {
        var article = TheJournalist.Exposes(() => TheTest());
        WriteAsserts(article, false);
        WriteReport<InvariantsSpike>(article, false);
        ExplainMe<InvariantsSpike>(false);
        Verify(article);
    }

    [CodeSnippet]
    private static void TheTest()
    {
        Check.ForAll(
            from len in Fuzzr.Int(0, 10)
            from elements in Fuzzr.Int().Many(len)
            select elements.ToList(),
            list => list.SequenceEqual(TheReverser.Reverses(TheReverser.Reverses(list))));
    }

    private static void Verify(Article article)
    {

    }
}