using QuickCheckr.UnderTheHood;
using QuickCheckr.UnderTheHood.Docketing;

namespace QuickCheckr;

public static partial class Checkr
{
    /// <summary>
    /// Adds a named action step to the Checkr script and captures its result for later specs and traces.
    /// Use when you want a synchronous operation to appear as an executable step in the report.
    /// </summary>
    public static CheckrOf<T> Act<T>(string key, Func<T> func)
        => TryCatch(key, func);

    /// <summary>
    /// Adds a named asynchronous action step to the Checkr script and captures its result for later specs and traces.
    /// Use when you want to await a task as part of the Checkr workflow without exposing async plumbing in your test.
    /// </summary>
    public static CheckrOf<T> Act<T>(string key, Task<T> func)
        => TryCatch(key, () => func.GetAwaiter().GetResult());

    /// <summary>
    /// Adds a named action step to the Checkr script for side-effect-only operations.
    /// Use when you want the operation to show up in the report but do not need a value from it.
    /// </summary>
    public static CheckrOf<Case> Act(string key, Action action)
        => TryCatch(key, () => { action(); return Case.Closed; });

    /// <summary>
    /// Adds a named asynchronous action step to the Checkr script for side-effect-only operations.
    /// Use when you want an async operation to be part of the run and visible in the report, without caring about a result value.
    /// </summary>
    public static CheckrOf<Case> Act(string key, Task action)
        => TryCatch(key, () => { action.GetAwaiter().GetResult(); return Case.Closed; });

    /// <summary>
    /// Adds a named action step that only executes when a condition holds, capturing its result for later specs and traces.
    /// Use when a synchronous operation is optional but should be reported whenever it runs.
    /// </summary>
    public static CheckrOf<T> ActIf<T>(this string key, Func<bool> predicate, Func<T> func)
        => state => predicate() ? Act(key, func)(state) : CheckrResult.Default<T>(state);

    /// <summary>
    /// Adds a named asynchronous action step that only executes when a condition holds, capturing its result for later specs and traces.
    /// Use when an async operation is optional but should still appear as a proper step whenever it runs.
    /// </summary>
    public static CheckrOf<T> ActIf<T>(this string key, Func<bool> predicate, Task<T> func)
        => state => predicate() ? Act(key, func)(state) : CheckrResult.Default<T>(state);

    /// <summary>
    /// Adds a named action step that only executes when a condition holds, for side-effect-only operations.
    /// Use when the operation is optional and you simply want the case to continue unchanged when it is skipped.
    /// </summary>
    public static CheckrOf<Case> ActIf(this string key, Func<bool> predicate, Action func)
        => state => predicate() ? Act(key, func)(state) : CheckrResult.CaseOnly(state);

    /// <summary>
    /// Adds a named asynchronous action step that only executes when a condition holds, for side-effect-only operations.
    /// Use when an async operation is optional and the case should continue unchanged when it is skipped.
    /// </summary>
    public static CheckrOf<Case> ActIf(this string key, Func<bool> predicate, Task func)
        => state => predicate() ? Act(key, func)(state) : CheckrResult.CaseOnly(state);


    private static CheckrOf<T> TryCatch<T>(string key, Func<T> func) =>
        state =>
        {
            state.Memory.StoreAction(key);
            try
            {
                return CheckrResult.Some(state, func());
            }
            catch (Exception exception)
            {
                state.Docket.Record(new EvidenceFailure(exception));
                state.EvidenceRecorder.RecordFailure(exception);
                return CheckrResult.Default<T>(state);
            }
        };

}