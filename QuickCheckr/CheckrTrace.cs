using QuickCheckr.UnderTheHood;

namespace QuickCheckr;

public static partial class Checkr
{
    /// <summary>
    /// Records named trace lines for inclusion in the report.
    /// Use the supplied context to format values, or return lines directly to control their layout.
    /// </summary>
    public static CheckrOf<Case> Trace(string key, Func<TraceContext, string[]> trace) =>
        state =>
        {
            state.Memory.StoreTrace(key, () => trace(new TraceContext(state.Editor)));
            return CheckrResult.CaseOnly(state);
        };
}
