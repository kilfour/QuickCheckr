using QuickCheckr.UnderTheHood.Warnings;

namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.InputReduction;

internal static class ReductionRunner
{
    public static async ValueTask<Reduction> ForReducerAsync(
        Func<ReductionResult, Reduction> reducer,
        string key,
        string path,
        object? value,
        Func<object, ValueTask<bool>> failTest,
        IReductionWhistleblower whistleblower)
    {
        var first = new ReductionResult(true, value);
        var reduction = reducer(first);
        bool failed;
        var cnt = 0;
        while (reduction is Reduction.Next next)
        {
            if (cnt > whistleblower.MaxStepsPerReducer)
            {
                whistleblower.Warn(key,
                    () => new MaxStepsExceededWarning(path, whistleblower.MaxStepsPerReducer));
                return new Reduction.Unbounded(next.Candidate);
            }
            failed = await failTest(next.Candidate!).ConfigureAwait(false);
            if (failed)
                first = new ReductionResult(true, next.Candidate!);
            else
                first = new ReductionResult(false, first.Value!);
            reduction = reducer(first);
            cnt++;
        }
        return reduction;
    }
}
