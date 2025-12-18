using QuickCheckr.UnderTheHood.Docketing;

namespace QuickCheckr.UnderTheHood;

public static class Evaluate
{
    public static CheckrOf<Case> Expect(string key, Func<bool> condition) =>
        state =>
        {
            state.Memory.ExpectGuard.Check(key);
            if (state.EvidenceRecorder.ExpectationNeedsToBeSkipped(key))
                return CheckrResult.CaseOnly(state);
            return TheCatcher.InTheCheckr(state, $"Spec: {key}",
                () => InnerExpect(key, condition, state),
                (ex) => state.EvidenceRecorder.ExpectationCrashed(ex));
        };

    private static CheckrResult<Case> InnerExpect(string key, Func<bool> condition, CheckrState state)
    {
        if (condition())
            state.EvidenceRecorder.ExpectationPassed(key);
        else
            state.EvidenceRecorder.ExpectationFailed(key);
        return CheckrResult.CaseOnly(state);
    }


}

public static class TheCatcher
{
    public static CheckrResult<T> InTheCheckr<T>(
        CheckrState state,
        string key,
        Func<CheckrResult<T>> func,
        Action<Exception>? onException = null)
    {
        try
        {
            return func();
        }
        catch (ActionWasNotExecuted)
        {
            state.Docket.Record(new SpecCaughtActionWasNotExecuted(key));
            return CheckrResult.Default<T>(state);
        }
        catch (Exception exception)
        {
            state.Docket.Record(new EvidenceFailure(exception));
            onException?.Invoke(exception);
            return CheckrResult.Default<T>(state);
        }
    }
}