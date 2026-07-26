using QuickCheckr.UnderTheHood;

namespace QuickCheckr;

public static partial class Trackr
{
    /// <summary>
    /// Evaluates a named condition only on the final execution of a run.  
    /// Use when an assertion depends on the cumulative state built up across all executions.
    /// </summary>
    public static CheckrOf<Case> Verify(string key, Func<bool> condition)
        => InnerVerify(key, condition, () => []);

    public static CheckrOf<Case> Verify(string key, Func<bool> condition, Func<string> message)
        => InnerVerify(key, condition, () => [message()]);

    public static CheckrOf<Case> Verify(string key, Func<bool> condition, Func<List<string>>? messages)
        => InnerVerify(key, condition, messages);

    private static CheckrOf<Case> InnerVerify(
        string key,
        Func<bool> condition,
        Func<List<string>>? messages) =>
        state =>
        {
            state.Memory.KeyGuardian.Expects.Check(key);
            if (!state.IsThisTheRunsLastExecution())
                return CheckrResult.CaseOnly(state);
            bool passed = condition();
            if (!passed)
            {
                state.EvidenceRecorder.VerificationFailed(
                    new VerificationFailure(key, messages?.Invoke() ?? []));
            }
            else
            {
                state.EvidenceRecorder.ExpectationPassed(key);
            }
            return CheckrResult.CaseOnly(state);
        };

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
}
