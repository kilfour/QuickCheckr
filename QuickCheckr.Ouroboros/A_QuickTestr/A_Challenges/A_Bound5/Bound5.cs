using QuickCheckr.Invariants;
using QuickCheckr.Tests.Tools.ThePress;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickCheckr.UnderTheHood;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Ouroboros.A_QuickTestr.A_Challenges.A_Bound5;

using Tuple5ShortLists = (List<short>, List<short>, List<short>, List<short>, List<short>);

[DocFile]
[DocContent(
@"Given a 5-tuple of lists of 16-bit integers, we want to test the property that if each list sums to less than 256,
then the sum of all the values in the lists is less than 5 * 256.
This is false because of overflow. e.g. ([-20000], [-20000], [], [], []) is a counter-example.

The interesting thing about this example is the interdependence between separate parts of the sample data.
A single list in the tuple will never break the invariant, but you need at least two lists together.
This prevents most of trivial shrinking algorithms from getting close to a minimum example,
which would look somethink like ([-32768], [-1], [], [], []).
")]
public class Bound5 : OuroborosTest<Bound5>
{
    protected override bool Asserts => false;
    protected override bool Report => true;
    protected override bool Explain => false;

    [Fact(Skip = "wip")]
    [DocReportHeader]
    [DocReport]
    public override void Example() =>
        Run(1000.Runs());

    [Fact(Skip = "wip")]
    public void Example2()
    { //510558750
        var checkr =
            from testr in Trackr.Stashed(() => TheTestr())
            from caseFile in Checkr.ActCarefully("Run Tester", () => testr.Run(2000.Runs()))
            from article in Checkr.Capture(() => TheJournalist.Investigates(() => GetCaseFile(caseFile)))
            from traceSeed in Checkr.Trace("Testr seed", () => article.Seed())
            from trace in Checkr.Trace("Testr input", () => GetInputString(article))
            from traceCount in Checkr.Trace("Element Count", () => ElementCount(GetInputValue(article)))
            from expect in Checkr.Expect("Element Count = 2", () => ElementCount(GetInputValue(article)) == 2)
            from traceCount2 in Checkr.Trace("Element Count again", () => ElementCount(GetInputValue(article)))
            select Case.Closed;
        checkr.Run(50.Runs());
    }

    private static UnderTheHood.Proceedings.CaseFile GetCaseFile(DelayedResult<UnderTheHood.Proceedings.CaseFile> caseFile)
        => ((FalsifiableException)caseFile.Exception!).CaseFile;

    private static Tuple5ShortLists GetInputValue(Article article)
        => (Tuple5ShortLists)article.Execution(1).Input(1).Read().InputValue!;

    private static string GetInputString(Article article)
        => article.Execution(1).Input(1).Read().Value?.ToString()!;

    private static readonly FuzzrOf<List<short>> ShortList =
        Fuzzr.Short(short.MinValue, 256)
            .Many(0, 10)
            .Where(a => Sum(a) < 256)
            .ToList();

    [CodeSnippet]
    protected override ITestrRunner TheTestr() =>
        Testr.Named("The sum of all values is less than 5 * 256.")
            .For(Fuzzr.Tuple(ShortList, ShortList, ShortList, ShortList, ShortList))
            .Deliberate(ElementCount, 2)
            .Assert(a => Sum(a) < (5 * 256));

    private static int ElementCount((List<short>, List<short>, List<short>, List<short>, List<short>) a)
        => a.Item1.Count + a.Item2.Count + a.Item3.Count + a.Item4.Count + a.Item5.Count;

    private static short Sum((List<short>, List<short>, List<short>, List<short>, List<short>) a)
        => (short)(Sum(a.Item1) + Sum(a.Item2) + Sum(a.Item3) + Sum(a.Item4) + Sum(a.Item5));

    private static short Sum(IEnumerable<short> list)
    {
        short sum = 0;
        foreach (var value in list)
            sum = (short)(sum + value);
        return sum;
    }

    protected override void Verify(Article article)
    {

    }
}
