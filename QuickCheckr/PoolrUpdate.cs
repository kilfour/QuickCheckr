using QuickCheckr.UnderTheHood;

namespace QuickCheckr;

public static partial class Poolr
{
    /// <summary>
    /// Selects a pool element, runs a synchronous or asynchronous Checkr to produce its replacement,
    /// and stores the replacement under the selected element's identifier.
    /// </summary>
    public static CheckrOf<Case> Update<T>(string key, Func<T, CheckrOf<T>> poolElement) =>
        from original in OneOfElement<T>(key)
        from result in new CheckrOf<Case>(state => TheCatcher.InTheCheckrAsync(state,
            () =>
                (from replacement in poolElement(original.Value)
                 from stored in UpdateElement(original.Id, replacement)
                 select Case.Closed)(state),
            ex => state.EvidenceRecorder.RecordFailure(ex)))
        select result;

    private static CheckrOf<Case> UpdateElement<T>(int id, T element) =>
        state =>
        {
            state.Memory.GetPoolDictionary<T>()[id] = element;
            return CheckrResult.CaseOnly(state);
        };
}
