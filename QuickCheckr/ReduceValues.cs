using QuickCheckr.UnderTheHood.Runners.InputReduction;

namespace QuickCheckr;

public static partial class Reduce
{
    public static Func<Reducer<T>> Values<T>(IEnumerable<T> values, bool checkAll = false)
        => checkAll
            ? () => new ReportLastFailureBuilder<T>(_ => values).Reduce
            : () => new StopOnFirstPassedBuilder<T>(_ => values).Reduce;
}
