using QuickCheckr.UnderTheHood;

namespace QuickCheckr;

public static partial class Trackr
{
    /// <summary>
    /// Evaluates a named condition only on the final execution of a run.  
    /// Use when an assertion depends on the cumulative state built up across all executions.
    /// </summary>
    public static CheckrOf<Case> ExpectAtEnd(string key, Func<bool> condition) =>
        state =>
        {
            state.Memory.SpecGuard.Check(key);
            if (!state.IsThisTheRunsLastExecution())
                return CheckrResult.CaseOnly(state);
            bool passed = condition();
            if (!passed)
            {
                state.EvidenceRecorder.AsssayFailed(key);
            }
            else
            {
                state.EvidenceRecorder.SpecPassed(key);
            }
            return CheckrResult.CaseOnly(state);
        };

    /// <summary>
    /// Evaluates a set of labelled conditions only on the final execution of a run.  
    /// Use when you want to report multiple related assertions under a single named assay.
    /// </summary>
    public static CheckrOf<Case> ExpectAtEnd(string key, params (string label, Func<bool> condition)[] conditions) =>
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
                state.EvidenceRecorder.AsssayFailed(key + ": " + string.Join(", ", strings));
            }
            else
            {
                state.EvidenceRecorder.SpecPassed(key);
            }
            return CheckrResult.CaseOnly(state);
        };
}