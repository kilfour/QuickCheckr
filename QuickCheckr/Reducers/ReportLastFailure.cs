using QuickCheckr.UnderTheHood.Runners.InputReduction;

namespace QuickCheckr.Reducers;

public static partial class Reducr
{
    public static Func<ReductionResult, Reduction> ReportLastFailure<T>(Func<T, IEnumerable<T>> factory)
        => new ReportLastFailureBuilder<T>(factory).Reduce;

    public class ReportLastFailureBuilder<T>(Func<T, IEnumerable<T>> factory)
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
