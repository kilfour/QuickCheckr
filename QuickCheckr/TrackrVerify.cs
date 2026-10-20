using QuickCheckr.UnderTheHood;

namespace QuickCheckr;

public static partial class Trackr
{
    /// <summary>
    /// Evaluates a named condition only on the final execution of a run.  
    /// Use when an assertion depends on the cumulative state built up across all executions.
    /// </summary>
    public static CheckrOf<Case> Verify(
        string key,
        Func<ExpectationContext, ExpectationResult> evaluation)
        => Evaluate.Verification(key, evaluation);

    public static CheckrOf<Case> Verify(string key, Func<bool> evaluation)
        => Verify(key, context => evaluation() ? context.Pass() : context.Fail());

    /// <summary>
    /// Evaluates a set of labelled conditions only on the final execution of a run.  
    /// Use when you want to report multiple related assertions under a single named verification.
    /// </summary>
    public static CheckrOf<Case> Verify(string key, params (string label, Func<bool> condition)[] conditions) =>
        state =>
        {
            if (!state.IsThisTheRunsLastExecution())
                return CheckrResult.CaseOnly(state);
            bool passed = true;
            var strings = new List<string>();
            foreach (var (label, condition) in conditions)
            {
                if (!condition())
                {
                    strings.Add(label);
                    passed = false;
                }
            }
            if (!passed)
            {
                state.EvidenceRecorder.VerificationFailed(
                    new VerificationFailure(key + ": " + string.Join(", ", strings), []));
            }
            else
            {
                state.EvidenceRecorder.ExpectationPassed(key);
            }
            return CheckrResult.CaseOnly(state);
        };

    public static CheckrOf<Case> Verify(
        string key,
        Func<ExpectationContext, Task<ExpectationResult>> evaluation)
        => Verify(key, (context, _) => evaluation(context));

    public static CheckrOf<Case> Verify(
        string key,
        Func<ExpectationContext, CancellationToken, Task<ExpectationResult>> evaluation)
        => Evaluate.VerificationAsync(key, evaluation);

    public static CheckrOf<Case> Verify(string key, Func<Task<bool>> evaluation)
        => Verify(key, async context =>
            await evaluation().ConfigureAwait(false)
                ? context.Pass()
                : context.Fail());

    public static CheckrOf<Case> Verify(string key, Func<CancellationToken, Task<bool>> evaluation)
        => Verify(key, async (context, token) =>
            await evaluation(token).ConfigureAwait(false)
                ? context.Pass()
                : context.Fail());

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
