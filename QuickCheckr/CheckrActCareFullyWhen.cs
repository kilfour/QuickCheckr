using QuickCheckr.UnderTheHood;

namespace QuickCheckr;

public static partial class Checkr
{
    /// <summary>Conditionally invokes an async function and captures its result or exception.</summary>
    public static CheckrOf<DelayedResult<T>> ActCarefullyWhen<T>(string key, Func<bool> predicate, Func<Task<T>> func)
        => state => predicate() ? ActCarefully(key, func)(state) : CheckrResult.Some(state, DelayedResult<T>.None());

    /// <summary>Conditionally invokes an async function with the run's cancellation token.</summary>
    public static CheckrOf<DelayedResult<T>> ActCarefullyWhen<T>(string key, Func<bool> predicate, Func<CancellationToken, Task<T>> func)
        => state => predicate() ? ActCarefully(key, func)(state) : CheckrResult.Some(state, DelayedResult<T>.None());

    public static CheckrOf<DelayedResult<T>> ActCarefullyWhen<T>(string key, Func<bool> predicate, Func<T> func)
        => state => predicate() ? ActCarefully(key, func)(state) : CheckrResult.Some(state, DelayedResult<T>.None());
}
