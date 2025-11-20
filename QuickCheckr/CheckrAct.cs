using QuickCheckr.UnderTheHood;

namespace QuickCheckr;

public static partial class Checkr
{
    public static CheckrOf<TOutput> Act<TOutput>(string key, Func<TOutput> func)
        => TryCatch(key, func);

    public static CheckrOf<Case> Act(string key, Action action)
        => TryCatch(key, () => { action(); return Case.Closed; });

    private static CheckrOf<TOutput> TryCatch<TOutput>(string key, Func<TOutput> func) =>
        state =>
        {
            if (ActionNeedsToBeSkipped(key, state))
                return CheckrResult.None<TOutput>(state);
            state.ShortTermMemory.StoreAction(key);
            try
            {
                return CheckrResult.Some(state, func());
            }
            catch (Exception ex)
            {
                state.EvidenceRecorder.RecordFailure(ex);
                return CheckrResult.None<TOutput>(state);
            }
        };

    private static bool ActionNeedsToBeSkipped(string key, CheckrState state)
        => state.EvidenceRecorder.IsReplay && !state.ShortTermMemory.GetActions().Contains(key);
}