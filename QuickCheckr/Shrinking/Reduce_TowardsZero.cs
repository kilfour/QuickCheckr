using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.InputReduction;

namespace QuickCheckr.Shrinking;

public static partial class Reduce
{
    public static Func<Reducer<int>> TowardsZero
        => () => new ReportLastFailureBuilder<int>(TowardsZeroFactory).Reduce;

    private static IEnumerable<int> TowardsZeroFactory(int input)
    {
        var local = input;
        while (local != 0)
        {
            if (local > 0)
                yield return --local;
            else
                yield return ++local;
        }
    }
}