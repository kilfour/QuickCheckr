using QuickTestr.Tests.Tools;
using QuickFuzzr;
using QuickPulse.Explains;
using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Authoring.ThePress;

namespace QuickTestr.Tests.Challenges.A_Bound5;

[DocFile]
[DocContent(
@"Given a 5-tuple of lists of 16-bit integers, we want to test the property that if each list sums to less than 256,
then the sum of all the values in the lists is less than 5 * 256.
This is false because of overflow. e.g. ([-20000], [-20000], [], [], []) is a counter-example.

The interesting thing about this example is the interdependence between separate parts of the sample data.
A single list in the tuple will never break the invariant, but you need at least two lists together.
This prevents most of trivial shrinking algorithms from getting close to a minimum example,
which would look something like ([-32768], [-1], [], [], []).
")]
public class Bound5 : QuickTestrPropertyTest<Bound5>
{
    protected override bool Asserts => false;
    protected override bool Report => true;
    protected override bool Explain => false;

    [Fact]
    [DocTestrHeader]
    [DocTestr]
    [DocBoldHeader("The Short List Fuzzr")]
    [DocExample(typeof(Bound5), nameof(ShortList))]
    [DocBoldHeader("The Helpers")]
    [DocExample(typeof(H))]
    [DocReportHeader]
    [DocReport]
    public override void Example() => Document();

    [CodeSnippet]
    private static readonly FuzzrOf<List<short>> ShortList =
        Fuzzr.Short(short.MinValue, 256)
            .Many(0, 10)
            .Where(a => H.Sum(a) < 256)
            .ToList();

    [CodeSnippet]
    [CodeRemoveJournalist]
    [CodeRemove("472166887")]
    protected override void GetTestr(Journalist journalist) =>
        Testr.Named("The sum of all values is less than 5 * 256.")
            .For(Fuzzr.Tuple(ShortList, ShortList, ShortList, ShortList, ShortList))
            .Deliberate(H.ElementCount, 2)
            .Assert(a => H.Sum(a) < (5 * 256))
            .StoreCaseFiles(journalist)
            .Run(472166887);

    [CodeExample]
    public class H
    {
        public static int ElementCount((List<short>, List<short>, List<short>, List<short>, List<short>) a)
            => a.Item1.Count + a.Item2.Count + a.Item3.Count + a.Item4.Count + a.Item5.Count;

        public static short Sum((List<short>, List<short>, List<short>, List<short>, List<short>) a)
            => (short)(Sum(a.Item1) + Sum(a.Item2) + Sum(a.Item3) + Sum(a.Item4) + Sum(a.Item5));

        public static short Sum(IEnumerable<short> list)
        {
            short sum = 0;
            foreach (var value in list)
                sum = (short)(sum + value);
            return sum;
        }
    }

    protected override void Verify(Article article)
    {
        Assert.Equal("The sum of all values is less than 5 * 256.", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Input", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("( _, _, _, [ -23457 ], [ -25242 ] )", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("( _, _, _, [ -7527 ], [ -25242 ] )", article.Execution(1).Input(1).Read().Redux.Value);
    }
}
