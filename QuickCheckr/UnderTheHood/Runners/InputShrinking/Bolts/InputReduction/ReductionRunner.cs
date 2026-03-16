namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.InputReduction;

public static class ReductionRunner
{
    public static Reduction ForReducer(Func<ReductionResult, Reduction> reducer, object? value, Func<object, bool> failTest)
    {
        var result = new ReductionResult(true, value);
        var step = reducer(result);
        bool failed;
        while (step.Continue)
        {
            failed = failTest(step.Candidate!);
            if (failed)
                result = new ReductionResult(true, step.Candidate!);
            else
                result = new ReductionResult(false, result.Value!);
            step = reducer(result);
        }
        return step;
    }
}
