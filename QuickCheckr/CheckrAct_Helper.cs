using QuickCheckr.UnderTheHood;

namespace QuickCheckr;

public static partial class Checkr
{
    private static CheckrOf<T> TryCatch<T>(string key, Func<T> func) =>
        state =>
        {
            // if (ActionNeedsToBeSkipped(key, state))
            //      throw new CheckrWasNotExecuted();
            if (state.IsInitialRun)
                state.Memory.StoreAction(key);
            return TheCatcher.InTheCheckr(state,
                () => CheckrResult.Some(state, func()),
                ex => state.EvidenceRecorder.RecordFailure(ex));
        };
    // private static bool ActionNeedsToBeSkipped(string key, CheckrState state)
    //     => state.EvidenceRecorder.IsReplay && !state.Memory.GetActions().Contains(key);
}
