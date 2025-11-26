using QuickCheckr.UnderTheHood;

namespace QuickCheckr;

public static partial class Checkr
{
    public static CheckrOf<DelayedResult> ActCarefully(this string key, Action action)
        => key.TryCapture(() => { action(); return new DelayedResult(); }, ex => new DelayedResult(ex));

    public static CheckrOf<DelayedResult<T>> ActCarefully<T>(this string key, Func<T> func)
        => key.TryCapture(() => new DelayedResult<T>(func()), ex => new DelayedResult<T>(ex));

    public static CheckrOf<DelayedResult<T>> ActCarefully<T>(this string key, Func<Task<T>> func)
    => key.TryCapture(
        () => new DelayedResult<T>(func().GetAwaiter().GetResult()),
        ex => new DelayedResult<T>(ex));

    private static CheckrOf<T> TryCapture<T>(this string key, Func<T> func, Func<Exception, T> onError) =>
        state =>
        {
            if (ActionNeedsToBeSkipped(key, state))
                return CheckrResult.Default<T>(state);
            state.Memory.StoreAction(key);
            try
            {
                return CheckrResult.Some(state, func());
            }
            catch (Exception ex)
            {
                return CheckrResult.Some(state, onError(ex));
            }
        };
}