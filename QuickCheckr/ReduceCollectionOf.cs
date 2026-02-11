using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.InputReduction;

namespace QuickCheckr;

public static partial class Reduce
{
    public static Func<Reducer<IEnumerable<T>>> CollectionOf<T>(Func<IEnumerable<T>, IEnumerable<IEnumerable<T>>> factory, bool checkAll = false)
        => checkAll
            ? () => new ReportLastFailureBuilder<IEnumerable<T>>(factory).Reduce
            : () => new StopOnFirstPassedBuilder<IEnumerable<T>>(factory).Reduce;
}
