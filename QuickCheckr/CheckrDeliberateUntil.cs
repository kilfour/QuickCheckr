using QuickCheckr.UnderTheHood;

namespace QuickCheckr;

public static partial class Checkr
{
    /// <summary>
    /// Stops an inline deliberation when its summed score reaches this target.
    /// </summary>
    public static CheckrOf<Case> DeliberateUntil(int target) =>
        state =>
        {
            state.Memory.StoreDeliberationTarget(target);
            return CheckrResult.CaseOnly(state);
        };
}
