using QuickCheckr.UnderTheHood.Runners.InputReduction;

namespace QuickCheckr;

public static partial class Reduce
{
    public static Reducer<int> TowardsZero
        => new StopOnFirstPassedBuilder<int>(TowardsZeroFactory).Reduce;

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