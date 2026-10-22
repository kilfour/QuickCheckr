using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.InputReduction;

namespace QuickCheckr.Shrinking;

public static partial class Reduce
{
    public static Func<Reducer<T>> Function<T>(Func<T, IEnumerable<T>> factory, bool checkAll = false)
        => checkAll
            ? () => new ReportLastFailureBuilder<T>(factory).Reduce
            : () => new StopOnFirstPassedBuilder<T>(factory).Reduce;
}
