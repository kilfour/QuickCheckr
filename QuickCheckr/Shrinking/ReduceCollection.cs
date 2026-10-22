using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.InputReduction;

namespace QuickCheckr.Shrinking;

public static partial class Reduce
{
    public static Func<Reducer<IEnumerable<T>>> Collection<T>(IEnumerable<IEnumerable<T>> values, bool checkAll = false)
        => checkAll
            ? () => new ReportLastFailureBuilder<IEnumerable<T>>(_ => values).Reduce
            : () => new StopOnFirstPassedBuilder<IEnumerable<T>>(_ => values).Reduce;
}
