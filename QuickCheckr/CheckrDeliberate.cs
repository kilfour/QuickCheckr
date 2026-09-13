using QuickCheckr.UnderTheHood;

namespace QuickCheckr;

public static partial class Checkr
{
    /// <summary>
    /// Declares a score used to select the preferred failing case from multiple runs.
    /// Lower scores are preferred. Scores from multiple clauses and retained
    /// executions are added together.
    /// </summary>
    public static CheckrOf<Case> Deliberate(Func<int> score) =>
        state =>
        {
            state.Memory.StoreDeliberation(score);
            return CheckrResult.CaseOnly(state);
        };
}
