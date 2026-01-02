using QuickCheckr.UnderTheHood.Runners.InputReduction;

namespace QuickCheckr.Reducers;

public static partial class Reducr
{
    public static Func<ReductionResult, Reduction> StopOnFirstPassed<T>(Func<T, IEnumerable<T>> factory)
        => new StopOnFirstPassedBuilder<T>(factory).Reduce;

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
}
