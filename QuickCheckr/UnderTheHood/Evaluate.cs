namespace QuickCheckr.UnderTheHood;

public static class Evaluate
{
    public static CheckrOf<Case> Expect(string key, Func<bool> condition, bool register, Func<List<string>>? message = null) =>
        state =>
        {
            if (register)
                state.Memory.KeyGuardian.Expects.Check(key);
            if (state.EvidenceRecorder.ExpectationNeedsToBeSkipped(key))
                return CheckrResult.CaseOnly(state);
            return TheCatcher.InTheCheckr(state,
                () => InnerExpect(key, condition, state, message),
                (ex) => state.EvidenceRecorder.ExpectationCrashed(key, ex));
        };

    private static CheckrResult<Case> InnerExpect(string key, Func<bool> condition, CheckrState state, Func<List<string>>? message)
    {
        if (condition())
            state.EvidenceRecorder.ExpectationPassed(key);
        else
        {
            var failure = new ExpectationFailure(key, message == null ? [] : message());
            state.EvidenceRecorder.ExpectationFailed(failure, state.Memory.ShortTermMemory.CurrentExecutionNumber);
        }
        return CheckrResult.CaseOnly(state);
    }
}
