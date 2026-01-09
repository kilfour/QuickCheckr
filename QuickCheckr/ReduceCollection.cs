using QuickCheckr.UnderTheHood.Runners.InputReduction;

namespace QuickCheckr;

public static partial class Reduce
{
    public static Func<Reducer<IEnumerable<T>>> Collection<T>(IEnumerable<IEnumerable<T>> values, bool checkAll = false)
        => checkAll
            ? () => new ReportLastFailureBuilder<IEnumerable<T>>(_ => values).Reduce
            : () => new StopOnFirstPassedBuilder<IEnumerable<T>>(_ => values).Reduce;


}
