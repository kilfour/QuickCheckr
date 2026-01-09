using QuickCheckr.UnderTheHood.Runners.InputReduction;

namespace QuickCheckr;

public static partial class Reduce
{
    public static Func<Reducer<T>> Function<T>(Func<T, IEnumerable<T>> factory, bool checkAll = false)
        => checkAll
            ? () => new ReportLastFailureBuilder<T>(factory).Reduce
            : () => new StopOnFirstPassedBuilder<T>(factory).Reduce;
}
