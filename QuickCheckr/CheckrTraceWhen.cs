using QuickCheckr.UnderTheHood;

namespace QuickCheckr;

public static partial class Checkr
{
    /// <summary>
    /// Records named trace lines only when a predicate holds.
    /// Use the supplied context to format values, or return lines directly to control their layout.
    /// </summary>
    public static CheckrOf<Case> TraceWhen(
        string key,
        Func<bool> predicate,
        Func<TraceContext, string[]> trace) =>
        state => predicate() ? Trace(key, trace)(state) : CheckrResult.CaseOnly(state);
}
