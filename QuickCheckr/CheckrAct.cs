using QuickCheckr.UnderTheHood;

namespace QuickCheckr;

public static partial class Checkr
{
    public static CheckrOf<T> Act<T>(string key, Func<T> func)
        => TryCatch(key, func);

    public static CheckrOf<Case> Act(string key, Action action)
        => TryCatch(key, () => { action(); return Case.Closed; });

    public static CheckrOf<T> ActIf<T>(this string key, Func<bool> predicate, Func<T> func)
        => state => predicate() ? Act(key, func)(state) : CheckrResult.Default<T>(state);

    public static CheckrOf<Case> ActIf(this string key, Func<bool> predicate, Action func)
        => state => predicate() ? Act(key, func)(state) : CheckrResult.CaseOnly(state);

    private static CheckrOf<T> TryCatch<T>(string key, Func<T> func) =>
        state =>
        {
            if (ActionNeedsToBeSkipped(key, state))
                return CheckrResult.Default<T>(state);
            state.ShortTermMemories.StoreAction(key);
            try
            {
                return CheckrResult.Some(state, func());
            }
            catch (Exception ex)
            {
                state.EvidenceRecorder.RecordFailure(ex);
                return CheckrResult.Default<T>(state);
            }
        };

    private static bool ActionNeedsToBeSkipped(string key, CheckrState state)
        => state.EvidenceRecorder.IsReplay && !state.ShortTermMemories.HasAction(key);
}