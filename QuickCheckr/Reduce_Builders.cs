using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.InputReduction;

namespace QuickCheckr;

public static partial class Reduce
{
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
            else
                return Reduction.Stop(lastFailure!);
            if (index >= values.Count)
            {
                index = 0;
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
            if (reductionResult.Failed)
            {
                lastFailure = reductionResult.ValueAs<T>();
            }
            if (index >= values.Count)
            {
                index = 0;
                return Reduction.Stop(lastFailure!);
            }
            return Reduction.Next(values[index++]!);
        }
    }
}
