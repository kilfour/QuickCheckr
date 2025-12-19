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
}
