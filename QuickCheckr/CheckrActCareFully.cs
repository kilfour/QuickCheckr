using QuickCheckr.UnderTheHood;

namespace QuickCheckr;

public static partial class Checkr
{
    /// <summary>
    /// Executes a named action while capturing any thrown exception into a <see cref="DelayedResult"/>.  
    /// Use when you want to assert on success or failure without letting the action abort the Checkr run.
    /// </summary>
    public static CheckrOf<DelayedResult> ActCarefully(string key, Action action)
        => key.TryCapture(
            () => { action(); return new DelayedResult(); },
            ex => new DelayedResult(ex));

    /// <summary>
    /// Executes a named asynchronous action while capturing any thrown exception into a <see cref="DelayedResult"/>.  
    /// Use when an awaited operation may fail and you want to check its outcome explicitly in later expectations.
    /// </summary>
    public static CheckrOf<DelayedResult> ActCarefully(string key, Task action)
        => key.TryCapture(
            () => { action.GetAwaiter().GetResult(); return new DelayedResult(); },
            ex => new DelayedResult(ex));

    /// <summary>
    /// Executes a named function while capturing either its result or any thrown exception.  
    /// Use when you want fine-grained assertions on both returned values and error conditions.
    /// </summary>
    public static CheckrOf<DelayedResult<T>> ActCarefully<T>(string key, Func<T> func)
        => key.TryCapture(
            () => new DelayedResult<T>(func()),
            ex => new DelayedResult<T>(ex));

    /// <summary>
    /// Executes a named asynchronous function while capturing either its result or any thrown exception.  
    /// Use when an awaited computation may succeed or fail and you want to record both paths for later expectations.
    /// </summary>
    public static CheckrOf<DelayedResult<T>> ActCarefully<T>(string key, Task<T> func)
        => key.TryCapture(
            () => new DelayedResult<T>(func.GetAwaiter().GetResult()),
            ex => new DelayedResult<T>(ex));
}