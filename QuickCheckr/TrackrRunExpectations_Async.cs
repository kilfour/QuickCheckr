using QuickCheckr.UnderTheHood;
namespace QuickCheckr;
public static partial class Trackr
{
    public static CheckrOf<Case> Expect(this string key, Func<Task<bool>> condition)
        => Expect(key, _ => condition());

    public static CheckrOf<Case> Expect(this string key, Func<CancellationToken, Task<bool>> condition)
        => state => state.IsThisTheRunsLastExecution()
            ? Evaluate.ExpectAsync(key, condition, true)(state) : CheckrResult.CaseOnly(state);

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
    public static CheckrOf<Case> Verify(string key, Func<Task<bool>> condition)
        => VerifyAsync(key, _ => condition(), null);

    public static CheckrOf<Case> Verify(string key, Func<Task<bool>> condition, Func<string> message)
        => VerifyAsync(key, _ => condition(), () => [message()]);

    public static CheckrOf<Case> Verify(string key, Func<Task<bool>> condition, Func<List<string>>? messages)
        => VerifyAsync(key, _ => condition(), messages);

    public static CheckrOf<Case> Verify(string key, Func<CancellationToken, Task<bool>> condition)
        => VerifyAsync(key, condition, null);

    public static CheckrOf<Case> Verify(string key, Func<CancellationToken, Task<bool>> condition, Func<string> message)
        => VerifyAsync(key, condition, () => [message()]);

    public static CheckrOf<Case> Verify(string key, Func<CancellationToken, Task<bool>> condition, Func<List<string>>? messages)
        => VerifyAsync(key, condition, messages);


    private static CheckrOf<Case> VerifyAsync(string key, Func<CancellationToken, Task<bool>> condition,
        Func<List<string>>? messages) =>
        async state =>
        {
            state.Memory.KeyGuardian.Expects.Check(key);
            if (!state.IsThisTheRunsLastExecution())
                return CheckrResult.CaseOnly(state);
            state.CancellationToken.ThrowIfCancellationRequested();
            var passed = await condition(state.CancellationToken).ConfigureAwait(false);
            state.CancellationToken.ThrowIfCancellationRequested();
            if (passed) state.EvidenceRecorder.ExpectationPassed(key);
            else state.EvidenceRecorder.VerificationFailed(new VerificationFailure(key, messages?.Invoke() ?? []));
            return CheckrResult.CaseOnly(state);
        };

    public static CheckrOf<Case> Verify(string key)
        => Verify(key, Array.Empty<(string label, Func<bool> condition)>());

    public static CheckrOf<Case> Verify(string key, params (string label, Func<Task<bool>> condition)[] conditions)
        => Verify(key, conditions.Select(item =>
            (item.label, (Func<CancellationToken, Task<bool>>)(_ => item.condition()))).ToArray());

    public static CheckrOf<Case> Verify(string key,
        params (string label, Func<CancellationToken, Task<bool>> condition)[] conditions) =>
        async state =>
        {
            if (!state.IsThisTheRunsLastExecution())
                return CheckrResult.CaseOnly(state);
            List<string> failed = [];
            foreach (var (label, condition) in conditions)
            {
                state.CancellationToken.ThrowIfCancellationRequested();
                var passed = await condition(state.CancellationToken).ConfigureAwait(false);
                state.CancellationToken.ThrowIfCancellationRequested();
                if (!passed) failed.Add(label);
            }
            if (failed.Count == 0) state.EvidenceRecorder.ExpectationPassed(key);
            else state.EvidenceRecorder.VerificationFailed(
                new VerificationFailure(key + ": " + string.Join(", ", failed), []));
            return CheckrResult.CaseOnly(state);
        };
}
