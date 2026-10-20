using QuickCheckr.UnderTheHood;

namespace QuickCheckr;

public static partial class Trackr
{
    /// <summary>
    /// Marks a named condition as proven when it first evaluates to true and stops the current run early.  
    /// Use for properties that require only one successful observation instead of completing all remaining executions.
    /// </summary>
    public static CheckrOf<Case> StopWhenProven(string key, Func<bool> condition) =>
        state =>
        {
            state.Memory.KeyGuardian.Expects.Check(key);
            if (state.EvidenceRecorder.ExpectationNeedsToBeSkipped(key))
                return CheckrResult.CaseOnly(state);
            return TheCatcher.InTheCheckr(state,
                () => EvaluateStopWhenProven(key, condition, state),
                ex => state.EvidenceRecorder.RecordFailure(ex));
        };

    public static CheckrOf<Case> StopWhenProven(string key, Func<Task<bool>> condition)
        => StopWhenProven(key, _ => condition());

    public static CheckrOf<Case> StopWhenProven(string key, Func<CancellationToken, Task<bool>> condition) =>
        state =>
        {
            state.Memory.KeyGuardian.Expects.Check(key);
            if (state.EvidenceRecorder.ExpectationNeedsToBeSkipped(key))
                return CheckrResult.CaseOnly(state);
            return TheCatcher.InTheCheckrAsync(state, async () =>
            {
                var passed = await condition(state.CancellationToken).ConfigureAwait(false);
                state.CancellationToken.ThrowIfCancellationRequested();
                return EvaluateStopWhenProven(key, () => passed, state);
            }, ex => state.EvidenceRecorder.RecordFailure(ex));
        };

    private static CheckrResult<Case> EvaluateStopWhenProven(string key, Func<bool> condition, CheckrState state)
    {
        if (condition())
        {
            state.EvidenceRecorder.ExpectationPassed(key);
            state.StopRun();
        }
        return CheckrResult.CaseOnly(state);
    }
}