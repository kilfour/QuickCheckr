namespace QuickCheckr.UnderTheHood.Runners.InputReduction;

public class SimpleReducer<T>(Func<T, IEnumerable<T>> factory)
{
    private List<T>? values = null;
    private int index = 0;
    private T? lastFailure = default;
    private bool final;

    public Reduction Reducer(ReductionResult reductionResult)
    {
        if (final)
            return Reduction.Stop();
        values ??= [.. factory(reductionResult.ValueAs<T>())];
        if (index >= values.Count)
        {
            final = true;
            return Reduction.Next(lastFailure!);
        }
        if (reductionResult.Failed)
        {
            lastFailure = reductionResult.ValueAs<T>();
        }
        return Reduction.Next(values[index++]!);
    }
}
