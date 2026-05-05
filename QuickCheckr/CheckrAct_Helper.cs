using QuickCheckr.UnderTheHood;

namespace QuickCheckr;

public static partial class Checkr
{
    private static CheckrOf<T> TryCatch<T>(string key, Func<T> func) =>
        state =>
        {
            if (!state.EvidenceRecorder.IsReplay) // HACK
                state.Memory.StoreAction(key);
            return TheCatcher.InTheCheckr(state,
                () =>
                {
                    // if (ActionNeedsToBeSkipped(key, state))
                    //     throw new CheckrWasNotExecuted();
                    return CheckrResult.Some(state, func());
                },
                ex => state.EvidenceRecorder.RecordFailure(ex));
        };
    private static bool ActionNeedsToBeSkipped(string key, CheckrState state)
        => state.EvidenceRecorder.IsReplay && !state.Memory.GetActions().Contains(key);
}
