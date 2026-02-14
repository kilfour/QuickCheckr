using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.InputReduction;

namespace QuickCheckr;

public static partial class Reduce
{
    public static Func<Reducer<int>> Towards(int target)
        => () => new StopOnFirstPassedBuilder<int>(a => TowardsFactory(target, a)).Reduce;

    private static IEnumerable<int> TowardsFactory(int target, int input)
    {
        var local = input;
        while (local != target)
        {
            if (local > target)
                yield return --local;
            else
                yield return ++local;
        }
    }
}