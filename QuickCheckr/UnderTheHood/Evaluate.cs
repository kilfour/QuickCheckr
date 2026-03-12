namespace QuickCheckr.UnderTheHood;

public static class Evaluate
{
    public static CheckrOf<Case> Expect(string key, Func<bool> condition) =>
        state =>
        {
            state.Memory.KeyGuardian.Expects.Check(key);
            if (state.EvidenceRecorder.ExpectationNeedsToBeSkipped(key))
                return CheckrResult.CaseOnly(state);
            return TheCatcher.InTheCheckr(state, $"Spec: {key}",
                () => InnerExpect(key, condition, state),
                (ex) => state.EvidenceRecorder.ExpectationCrashed(key, ex));
        };

    private static CheckrResult<Case> InnerExpect(string key, Func<bool> condition, CheckrState state)
    {
        try
        {
            if (condition())
                state.EvidenceRecorder.ExpectationPassed(key);
            else
                state.EvidenceRecorder.ExpectationFailed(key, state.Memory.ShortTermMemory.CurrentExecutionNumber);
        }
        catch (DelayedResultNullValueAccessed ex)
        {
            state.EvidenceRecorder.RecordFailure(ex);
        }

        return CheckrResult.CaseOnly(state);
    }
}