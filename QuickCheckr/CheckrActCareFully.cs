using QuickCheckr.UnderTheHood;

namespace QuickCheckr;

public static partial class Checkr
{
    /// <summary>Awaits a fresh action on each execution, capturing non-cancellation exceptions.</summary>
    public static CheckrOf<DelayedResult> ActCarefully(string key, Func<Task> action)
        => ActCarefully(key, _ => action());

    /// <summary>Awaits a fresh function on each execution, capturing non-cancellation exceptions.</summary>
    public static CheckrOf<DelayedResult<T>> ActCarefully<T>(string key, Func<Task<T>> func)
        => ActCarefully(key, _ => func());

    /// <summary>Awaits an action using the run's cancellation token.</summary>
    public static CheckrOf<DelayedResult> ActCarefully(string key, Func<CancellationToken, Task> action)
        => TryCaptureAsync(key,
            async token => { await action(token).ConfigureAwait(false); return new DelayedResult(); },
            ex => new DelayedResult(ex));

    /// <summary>Awaits a function using the run's cancellation token.</summary>
    public static CheckrOf<DelayedResult<T>> ActCarefully<T>(string key, Func<CancellationToken, Task<T>> func)
        => TryCaptureAsync(key,
            async token => new DelayedResult<T>(await func(token).ConfigureAwait(false)),
            ex => new DelayedResult<T>(ex));

    /// <summary>
    /// Executes a named action while capturing any thrown exception into a <see cref="DelayedResult"/>.  
    /// Use when you want to assert on success or failure without letting the action abort the Checkr run.
    /// </summary>
    public static CheckrOf<DelayedResult> ActCarefully(string key, Action action)
        => key.TryCapture(
            () => { action(); return new DelayedResult(); },
            ex => new DelayedResult(ex));


    /// <summary>
    /// Executes a named function while capturing either its result or any thrown exception.  
    /// Use when you want fine-grained assertions on both returned values and error conditions.
    /// </summary>
    public static CheckrOf<DelayedResult<T>> ActCarefully<T>(string key, Func<T> func)
        => key.TryCapture(
            () => new DelayedResult<T>(func()),
            ex => new DelayedResult<T>(ex));

}
