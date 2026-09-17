namespace QuickCheckr.UnderTheHood;

internal static class Evaluate
{
    public static CheckrOf<Case> Expect(
        string key,
        Func<bool> condition,
        bool register,
        Func<List<string>>? message = null,
        Func<ExceptionFailureIdentity?>? failureIdentity = null) =>
        state =>
        {
            if (register)
                state.Memory.KeyGuardian.Expects.Check(key);
            if (state.EvidenceRecorder.ExpectationNeedsToBeSkipped(key))
                return CheckrResult.CaseOnly(state);
            return TheCatcher.InTheCheckr(state,
                () => InnerExpect(key, condition, state, message, failureIdentity),
                (ex) => state.EvidenceRecorder.ExpectationCrashed(key, ex));
        };

    public static CheckrOf<Case> ExpectAsync(
        string key,
        Func<CancellationToken, Task<bool>> condition,
        bool register,
        Func<List<string>>? message = null,
        Func<CancellationToken, Task<bool>>? predicate = null) =>
        state =>
        {
            if (register)
                state.Memory.KeyGuardian.Expects.Check(key);
            if (state.EvidenceRecorder.ExpectationNeedsToBeSkipped(key))
                return CheckrResult.CaseOnly(state);
            return TheCatcher.InTheCheckrAsync(state,
                async () =>
                {
                    if (predicate is not null)
                    {
                        var applies = await predicate(state.CancellationToken).ConfigureAwait(false);
                        state.CancellationToken.ThrowIfCancellationRequested();
                        if (!applies) return CheckrResult.CaseOnly(state);
                    }
                    var passed = await condition(state.CancellationToken).ConfigureAwait(false);
                    state.CancellationToken.ThrowIfCancellationRequested();
                    return InnerExpect(key, () => passed, state, message, null);
                },
                ex => state.EvidenceRecorder.ExpectationCrashed(key, ex));
        };
    private static CheckrResult<Case> InnerExpect(
        string key,
        Func<bool> condition,
        CheckrState state,
        Func<List<string>>? message,
        Func<ExceptionFailureIdentity?>? failureIdentity)
    {
        if (condition())
            state.EvidenceRecorder.ExpectationPassed(key);
        else
        {
            var failure = new ExpectationFailure(
                key,
                message == null ? [] : message(),
                failureIdentity?.Invoke());
            state.EvidenceRecorder.ExpectationFailed(failure, state.Memory.ShortTermMemory.CurrentExecutionNumber);
        }
        return CheckrResult.CaseOnly(state);
    }
}
