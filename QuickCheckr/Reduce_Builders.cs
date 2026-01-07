using QuickCheckr.UnderTheHood.Runners.InputReduction;

namespace QuickCheckr;

public static partial class Reduce
{
    // private Reducer<T> Build<T>§
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
                return Reduction.Stop(lastFailure!);
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
                return Reduction.Stop(lastFailure!);
            }
            return Reduction.Next(values[index++]!);
        }
    }
}
