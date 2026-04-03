using QuickCheckr.UnderTheHood;

namespace QuickCheckr;

public static partial class Checkr
{
    /// <summary>
    /// Records a named trace value for inclusion in the report.  
    /// Use when you want to expose intermediate values or state to help explain shrinking or failures.
    /// </summary>
    public static CheckrOf<Case> Trace<T>(string key, Func<T?> factory) =>
        state =>
        {
            state.Memory.StoreTrace(key, () => state.Editor.IntroduceThis(factory()));
            return CheckrResult.CaseOnly(state);
        };

    /// <summary>
    /// Records a named trace string.  
    /// Use for arbitrary text or computed diagnostics that help illuminate the run.
    /// </summary>
    public static CheckrOf<Case> Trace(string key, Func<string> trace) =>
        state =>
        {
            state.Memory.StoreTrace(key, trace);
            return CheckrResult.CaseOnly(state);
        };
}