namespace QuickCheckr.UnderTheHood;

internal static class Evaluate
{
    public static CheckrOf<Case> Expectation(
        string key,
        Func<ExpectationContext, ExpectationResult> evaluate,
        bool register) =>
        state =>
        {
            if (register)
                RegisterExpectation(key, state);
            if (state.EvidenceRecorder.ExpectationNeedsToBeSkipped(key))
                return CheckrResult.CaseOnly(state);
            var context = new ExpectationContext(state.Editor);
            return TheCatcher.InTheCheckr(state,
                () => Expectation(key, evaluate(context), state),
                (ex) => state.EvidenceRecorder.ExpectationCrashed(key, ex));
        };

    public static CheckrOf<Case> ExpectationAsync(
        string key,
        Func<ExpectationContext, CancellationToken, Task<ExpectationResult>> evaluate,
        bool register) =>
        state =>
        {
            if (register)
                RegisterExpectation(key, state);
            if (state.EvidenceRecorder.ExpectationNeedsToBeSkipped(key))
                return CheckrResult.CaseOnly(state);
            var context = new ExpectationContext(state.Editor);
            return TheCatcher.InTheCheckrAsync(state,
                async () =>
                {
                    var result = await evaluate(context, state.CancellationToken).ConfigureAwait(false);
                    state.CancellationToken.ThrowIfCancellationRequested();
                    return Expectation(key, result, state);
                },
                ex => state.EvidenceRecorder.ExpectationCrashed(key, ex));
        };

    public static CheckrOf<Case> ExpectationWhen(
        string key,
        Func<bool> predicate,
        Func<ExpectationContext, ExpectationResult> evaluate,
        bool register) =>
        state =>
        {
            if (register)
                RegisterExpectation(key, state);
            if (state.EvidenceRecorder.ExpectationNeedsToBeSkipped(key))
                return CheckrResult.CaseOnly(state);
            var context = new ExpectationContext(state.Editor);
            return TheCatcher.InTheCheckr(state,
                () => predicate()
                    ? Expectation(key, evaluate(context), state)
                    : CheckrResult.CaseOnly(state),
                ex => state.EvidenceRecorder.ExpectationCrashed(key, ex));
        };

    public static CheckrOf<Case> ExpectationWhenAsync(
        string key,
        Func<CancellationToken, Task<bool>> predicate,
        Func<ExpectationContext, CancellationToken, Task<ExpectationResult>> evaluate,
        bool register) =>
        state =>
        {
            if (register)
                RegisterExpectation(key, state);
            if (state.EvidenceRecorder.ExpectationNeedsToBeSkipped(key))
                return CheckrResult.CaseOnly(state);
            var context = new ExpectationContext(state.Editor);
            return TheCatcher.InTheCheckrAsync(state,
                async () =>
                {
                    var applies = await predicate(state.CancellationToken).ConfigureAwait(false);
                    state.CancellationToken.ThrowIfCancellationRequested();
                    if (!applies)
                        return CheckrResult.CaseOnly(state);
                    var result = await evaluate(context, state.CancellationToken).ConfigureAwait(false);
                    state.CancellationToken.ThrowIfCancellationRequested();
                    return Expectation(key, result, state);
                },
                ex => state.EvidenceRecorder.ExpectationCrashed(key, ex));
        };

    public static CheckrOf<Case> Verification(
        string key,
        Func<ExpectationContext, ExpectationResult> evaluate) =>
        state =>
        {
            state.Memory.KeyGuardian.Expects.Check(key);
            if (!state.IsThisTheRunsLastExecution())
                return CheckrResult.CaseOnly(state);
            var context = new ExpectationContext(state.Editor);
            return Verification(key, evaluate(context), state);
        };

    public static CheckrOf<Case> VerificationAsync(
        string key,
        Func<ExpectationContext, CancellationToken, Task<ExpectationResult>> evaluate) =>
        async state =>
        {
            state.Memory.KeyGuardian.Expects.Check(key);
            if (!state.IsThisTheRunsLastExecution())
                return CheckrResult.CaseOnly(state);
            state.CancellationToken.ThrowIfCancellationRequested();
            var context = new ExpectationContext(state.Editor);
            var result = await evaluate(context, state.CancellationToken).ConfigureAwait(false);
            state.CancellationToken.ThrowIfCancellationRequested();
            return Verification(key, result, state);
        };

    private static void RegisterExpectation(string key, CheckrState state)
    {
        if (state.PoolIteration is { } iteration)
            iteration.Register(key);
        else
            state.Memory.KeyGuardian.Expects.Check(key);
    }

    internal static CheckrResult<Case> Expectation(
        string key,
        ExpectationResult result,
        CheckrState state)
    {
        if (state.PoolIteration is { } iteration)
        {
            iteration.Record(key, result);
            return CheckrResult.CaseOnly(state);
        }
        if (result.IsPassed)
        {
            state.EvidenceRecorder.ExpectationPassed(key);
        }
        else
        {
            state.EvidenceRecorder.ExpectationFailed(
                new ExpectationFailure(key, result.Messages, result.FailureIdentity?.Invoke()),
                state.Memory.ShortTermMemory.CurrentExecutionNumber);
        }
        return CheckrResult.CaseOnly(state);
    }

    private static CheckrResult<Case> Verification(
        string key,
        ExpectationResult result,
        CheckrState state)
    {
        if (result.IsPassed)
        {
            state.EvidenceRecorder.ExpectationPassed(key);
        }
        else
        {
            state.EvidenceRecorder.VerificationFailed(
                new VerificationFailure(key, result.Messages));
        }
        return CheckrResult.CaseOnly(state);
    }

}
