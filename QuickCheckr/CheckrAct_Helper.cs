using QuickCheckr.UnderTheHood;

namespace QuickCheckr;

public static partial class Checkr
{
    private static CheckrOf<T> TryCatch<T>(string key, Func<T> func) =>
        state =>
        {
            state.Memory.StoreAction(key);
            return TheCatcher.InTheCheckr(state, key,
                () => CheckrResult.Some(state, func()),
                ex => state.EvidenceRecorder.RecordFailure(ex));
        };

}
