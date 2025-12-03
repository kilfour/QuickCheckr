using QuickCheckr.UnderTheHood;
using QuickPulse.Show;

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
            state.Memory.StoreTrace(key, () => state.ManOfWealthAndTaste.IntroduceThis(factory()));
            return CheckrResult.CaseOnly(state);
        };

    // / <summary>
    // / Records a named trace value using a custom formatter.  
    // / Use when you want full control over how an object is rendered in the report.
    // / </summary>
    // public static CheckrOf<Case> Trace<T>(string key, T? obj, Func<string, string> formatter) =>
    //     state =>
    //     {
    //         state.Memory.StoreTrace(key, () => formatter(state.ManOfWealthAndTaste.IntroduceThis(obj)));
    //         return CheckrResult.CaseOnly(state);
    //     };

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

    /// <summary>
    /// Records a named trace string only when a predicate holds.  
    /// Use when a diagnostic is meaningful only in certain conditions and should be omitted otherwise.
    /// </summary>
    public static CheckrOf<Case> TraceIf(string key, Func<bool> predicate, Func<string> trace) =>
        state => predicate() ? Trace(key, trace)(state) : CheckrResult.CaseOnly(state);

    /// <summary>
    /// Records a named trace value only when a predicate holds.  
    /// Use when you want to expose intermediate values or state to help explain shrinking or failures.
    /// </summary>
    public static CheckrOf<Case> TraceIf<T>(string key, Func<bool> predicate, Func<T?> factory) =>
        state => predicate() ? Trace(key, factory)(state) : CheckrResult.CaseOnly(state);

}