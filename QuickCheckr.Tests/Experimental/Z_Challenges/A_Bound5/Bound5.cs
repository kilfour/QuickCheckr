using QuickCheckr.Protocol;
using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.InputReduction;
using QuickFuzzr;
using QuickPulse.Explains;
using QuickPulse.Show;

namespace QuickCheckr.Tests.Experimental.Z_Challenges.K_NestedLists;

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
    // [DocConfigureHeader]
    // [DocConfigure]
    [DocReportHeader]
    [DocReport]
    public override void Example() =>
        Run(100.Runs());

    // [CodeSnippet]
    // protected override CheckrConfig Configure(CheckrConfig config) =>
    //     config with
    //     {
    //         DeliberationPolicy = caseFile =>
    //             caseFile.InputsNamed<List<List<int>>>("Nested List", a => a.Count),
    //     };

    [CodeSnippet]
    private static readonly FuzzrOf<List<short>> InnerFuzzr =
         from list in Fuzzr.Short(short.MinValue, 100).Many(0, 10).Where(a => Sum(a) < 256)
         select list.ToList();

    [CodeSnippet]
    private static readonly FuzzrOf<List<List<short>>> TheFuzzr =
         from list in InnerFuzzr.Many(5)
         select list.ToList();

    private static readonly FuzzrOf<List<List<short>>> TheFuzzrConstant =
         Fuzzr.Constant(new List<List<short>> { new() { -20000 }, new() { -20000 }, new() { 1000 }, new(), new() });

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from input in Checkr.Input("Bound5", TheFuzzr) // ,Shrink.OnType<short>(s => s.Simplify(() => new ReduceShort().TowardsZero)))
        from trace in Checkr.Trace("Original", () => input)
        from expect in Checkr.Expect(
            "The sum of all values is less than 5 * 256.",
            () => input.Count == 5 && Sum(input) < 5 * 256)
        select Case.Closed;


    // private class ReduceShort
    // {
    //     private int index = 0;
    //     public Reduction TowardsZero(ReductionResult result)
    //     {
    //         var str = result.ValueAs<string>();
    //         if (result.Passed)
    //             index++;
    //         if (index >= str.Length)
    //             return Reduction.Stop(str);
    //         return Reduction.Next(str.Remove(index, 1));
    //     }
    // }

    private class ReduceShort
    {
        private short left;
        private short right;
        private bool initialized;
        private bool positive;

        public Reduction TowardsZero(ReductionResult result)
        {
            var current = result.ValueAs<short>();

            if (!initialized)
            {
                initialized = true;

                if (current == 0)
                    return Reduction.Stop(current);

                positive = current > 0;

                if (positive)
                {
                    left = 0;
                    right = current;
                }
                else
                {
                    left = current;
                    right = 0;
                }

                var first = Mid(left, right);

                return first == current
                    ? Reduction.Stop(current)
                    : Reduction.Next(first);
            }

            if (positive)
            {
                if (result.Passed) left = current;
                else right = current;

                if (right - left <= 1)
                    return Reduction.Stop(right);
            }
            else
            {
                if (result.Passed) right = current;
                else left = current;

                if (right - left <= 1)
                    return Reduction.Stop(left);
            }

            var next = Mid(left, right);

            return next == current
                ? Reduction.Stop(current)
                : Reduction.Next(next);
        }

        private static short Mid(short a, short b)
            => (short)(a + ((b - a) / 2));
    }

    private static int Sum(List<List<short>> list)
    {
        int sum = 0;
        foreach (var inner in list)
            foreach (var value in inner)
                sum = (short)(sum + value);
        return sum;
    }

    private static int Sum(IEnumerable<short> list)
    {
        int sum = 0;
        foreach (var value in list)
            sum = (short)(sum + value);
        return sum;
    }

    protected override void Verify(Article article)
    {

    }
}