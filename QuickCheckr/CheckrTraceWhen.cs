using QuickCheckr.UnderTheHood;

namespace QuickCheckr;

public static partial class Checkr
{
    /// <summary>
    /// Records a named trace string only when a predicate holds.  
    /// Use when a diagnostic is meaningful only in certain conditions and should be omitted otherwise.
    /// </summary>
    public static CheckrOf<Case> TraceWhen(string key, Func<bool> predicate, Func<string> trace) =>
        state => predicate() ? Trace(key, trace)(state) : CheckrResult.CaseOnly(state);

    /// <summary>
    /// Records a named trace value only when a predicate holds.  
    /// Use when you want to expose intermediate values or state to help explain shrinking or failures.
    /// </summary>
    public static CheckrOf<Case> TraceWhen<T>(string key, Func<bool> predicate, Func<T?> factory) =>
        state => predicate() ? Trace(key, factory)(state) : CheckrResult.CaseOnly(state);

}