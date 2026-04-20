using QuickCheckr.Protocol;
using QuickCheckr.Tests.Tools;
using QuickCheckr.Authoring.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Drafts.M_HoneyIShrunkTheInputs.D_Challenges.A_Bound5;

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
public class Bound5 : CheckrTest<Bound5>
{
    protected override bool Asserts => false;
    protected override bool Report => false;
    protected override bool Explain => false;

    [Fact]
    [DocCheckrHeader]
    [DocCheckr]
    [DocConfigureHeader]
    [DocConfigure]
    [DocReportHeader]
    [DocReport]
    public override void Example() =>
        Run(1866106578); // needs about 1000.Runs()

    [CodeSnippet]
    protected override CheckrConfig Configure(CheckrConfig config) =>
        config with
        {
            DeliberationPolicy = caseFile =>
                caseFile.InputsNamed<(List<short>, List<short>, List<short>, List<short>, List<short>)>(
                    "Bound5", a => ElementCount(a)),
            DeliberationTarget = 2
        };

    private static readonly FuzzrOf<List<short>> ShortList =
        Fuzzr.Short(short.MinValue, 256)
            .Many(0, 10)
            .Where(a => Sum(a) < 256)
            .ToList();

    private static readonly FuzzrOf<(List<short>, List<short>, List<short>, List<short>, List<short>)> TheFuzzr =
         Fuzzr.Tuple(ShortList, ShortList, ShortList, ShortList, ShortList);

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from input in Checkr.Input("Bound5", TheFuzzr)
        from trace in Checkr.Trace("Original", () => input)
        from expect in Checkr.Expect(
            "The sum of all values is less than 5 * 256.",
            () => Sum(input) < (5 * 256))
        select Case.Closed;

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
        Assert.Equal("The sum of all values is less than 5 * 256.", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(1, article.Total().Traces());
        Assert.Equal(0, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Bound5", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("( _, [ -21169 ], _, [ -13193 ], _ )", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("Original", article.Execution(1).Trace(1).Read().Label);
        Assert.Equal("( [ ], [ -8290, -21169 ], [ ], [ -13193 ], [ ] )", article.Execution(1).Trace(1).Read().Value);
    }
}
