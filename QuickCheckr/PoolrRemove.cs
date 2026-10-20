using QuickCheckr.UnderTheHood;

namespace QuickCheckr;

public static partial class Poolr
{
    /// <summary>
    /// Selects a pool element, runs a synchronous or asynchronous Checkr and then removes said element.
    /// </summary>
    public static CheckrOf<Case> Remove<T>(string key, Func<T, CheckrOf<Case>> checkr) =>
        from element in OneOfElement<T>(key)
        from result in new CheckrOf<Case>(state => TheCatcher.InTheCheckrAsync(state,
            () => (
                from _ in checkr(element.Value)
                from remove in RemoveElement(element)
                select Case.Closed)(state),
            ex => state.EvidenceRecorder.RecordFailure(ex)))
        select result;

    private static CheckrOf<Case> RemoveElement<T>(PoolElement<T> element) =>
        state =>
        {
            state.Memory.GetPoolDictionary<T>().Remove(element.Id);
            return CheckrResult.CaseOnly(state);
        };
}
