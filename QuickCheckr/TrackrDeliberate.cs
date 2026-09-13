using QuickCheckr.FilingCabinet;
using QuickCheckr.UnderTheHood;

namespace QuickCheckr;

public static partial class Trackr
{
    /// <summary>
    /// Declares a score calculated once from the completed failing run.
    /// Lower scores are preferred and multiple scores are added together.
    /// </summary>
    public static CheckrOf<Case> Deliberate(Func<CaseFile, int> score) =>
        state =>
        {
            state.Memory.StoreRunDeliberation(score, state.IsThisTheRunsLastExecution());
            return CheckrResult.CaseOnly(state);
        };
}
