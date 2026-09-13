using QuickCheckr.UnderTheHood;

namespace QuickCheckr;

public static partial class Trackr
{
    /// <summary>
    /// Stops deliberation when the combined execution- and run-level score
    /// reaches this target.
    /// </summary>
    public static CheckrOf<Case> DeliberateUntil(int target) =>
        state =>
        {
            state.Memory.StoreDeliberationTarget(target, state.IsThisTheRunsLastExecution());
            return CheckrResult.CaseOnly(state);
        };
}
