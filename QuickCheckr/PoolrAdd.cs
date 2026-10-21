using QuickCheckr.UnderTheHood;
using QuickFuzzr;

namespace QuickCheckr;

public static partial class Poolr
{
    /// <summary>
    /// Runs a synchronous or asynchronous Checkr and adds its value to the pool under a generated identifier.
    /// </summary>
    public static CheckrOf<Case> Add<T>(string key, CheckrOf<T> poolElement) =>
        state => TheCatcher.InTheCheckrAsync(state,
            () => poolElement.Select(value =>
            {
                var id = state.Memory.StoreVariable(key,
                    () => Fuzzr.Counter(typeof(T).Name)(state.FuzzrState).Value);
                state.Memory.StorePoolTrace(key, () => $"{typeof(T).Name}-{id}");
                state.Memory.StoreInPool(id, value);
                return Case.Closed;
            })(state),
            ex => state.EvidenceRecorder.RecordFailure(ex));
}
