using QuickCheckr.UnderTheHood;

namespace QuickCheckr;

public static partial class Poolr
{
    /// <summary>
    /// Selects a pool element, runs a synchronous or asynchronous Checkr to produce its replacement,
    /// and stores the replacement under the selected element's identifier.
    /// </summary>
    public static CheckrOf<Case> Update<T>(string key, Func<T, CheckrOf<T>> poolElement) =>
        state => TheCatcher.InTheCheckrAsync(state,
            () =>
                (from original in OneOfElement<T>(key)
                 from replacement in poolElement(original.Value)
                 from stored in original.Replace(replacement)
                 select Case.Closed)(state),
            ex => state.EvidenceRecorder.RecordFailure(ex));
}
