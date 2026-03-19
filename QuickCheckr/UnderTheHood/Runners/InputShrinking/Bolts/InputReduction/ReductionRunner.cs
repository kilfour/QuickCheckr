namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.InputReduction;

public static class ReductionRunner
{
    public static Reduction ForReducer(Func<ReductionResult, Reduction> reducer, object? value, Func<object, bool> failTest)
    {
        var first = new ReductionResult(true, value);
        var reduction = reducer(first);
        bool failed;
        while (reduction is Reduction.Next next)
        {
            failed = failTest(next.Candidate!);
            if (failed)
                first = new ReductionResult(true, next.Candidate!);
            else
                first = new ReductionResult(false, first.Value!);
            reduction = reducer(first);
        }
        return reduction;
    }
}
