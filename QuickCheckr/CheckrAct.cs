using QuickCheckr.UnderTheHood;

namespace QuickCheckr;

public static partial class Checkr
{
    public static CheckrOf<T> Act<T>(string key, Func<T> func)
        => TryCatch(key, func);


    public static CheckrOf<Case> Act(string key, Action action)
        => TryCatch(key, () => { action(); return Case.Closed; });

    private static CheckrOf<T> TryCatch<T>(string key, Func<T> func) =>
        state =>
        {
            if (ActionNeedsToBeSkipped(key, state))
                return CheckrResult.Default<T>(state);
            state.ShortTermMemories.Current.StoreAction(key);
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
        => state.EvidenceRecorder.IsReplay && !state.ShortTermMemories.Current.GetActions().Contains(key);
}