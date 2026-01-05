using QuickCheckr.UnderTheHood.Runners.InputReduction;

namespace QuickCheckr;

public static partial class Reduce
{
    public static Func<ReductionResult, Reduction> Values<T>(IEnumerable<T> values, bool checkAll = false)
        => checkAll
            ? new ReportLastFailureBuilder<T>(_ => values).Reduce
            : new StopOnFirstPassedBuilder<T>(_ => values).Reduce;

    public static Func<ReductionResult, Reduction> Function<T>(Func<T, IEnumerable<T>> factory, bool checkAll = false)
        => checkAll
            ? new ReportLastFailureBuilder<T>(factory).Reduce
            : new StopOnFirstPassedBuilder<T>(factory).Reduce;

    private class StopOnFirstPassedBuilder<T>(Func<T, IEnumerable<T>> factory)
    {
        private List<T>? values = null;
        private int index = 0;
        private T? lastFailure = default;

        public Reduction Reduce(ReductionResult reductionResult)
        {
            values ??= [.. factory(reductionResult.ValueAs<T>())];
            if (reductionResult.Failed)
                lastFailure = reductionResult.ValueAs<T>();
            if (index >= values.Count)
            {
                return Reduction.Stop(lastFailure!);
            }
            return Reduction.Next(values[index++]!);
        }
    }

    private class ReportLastFailureBuilder<T>(Func<T, IEnumerable<T>> factory)
    {
        private List<T>? values = null;
        private int index = 0;
        private T? lastFailure = default;

        public Reduction Reduce(ReductionResult reductionResult)
        {
            values ??= [.. factory(reductionResult.ValueAs<T>())];
            if (index >= values.Count)
            {
                return Reduction.Stop(lastFailure!);
            }
            if (reductionResult.Failed)
            {
                lastFailure = reductionResult.ValueAs<T>();
            }
            return Reduction.Next(values[index++]!);
        }
    }
}
