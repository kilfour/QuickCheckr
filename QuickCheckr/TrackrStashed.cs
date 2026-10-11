using QuickCheckr.UnderTheHood;

namespace QuickCheckr;

public static partial class Trackr
{
    /// <summary>
    /// Stashes a value under a type-based key and returns the stored instance.  
    /// Use when you want a single shared instance of a type to persist across executions within a run.
    /// </summary>
    public static CheckrOf<T> Stashed<T>(Func<T> func)
        => Stashed(typeof(T).FullName!, func);

    /// <summary>
    /// Stashes a value under a custom key and returns the stored instance.  
    /// Use when you need multiple distinct stashed values of the same type or want a more descriptive identifier.
    /// </summary>
    public static CheckrOf<T> Stashed<T>(string key, Func<T> func) =>
        state => CheckrResult.Some(state, state.Memory.StoreStashed(key, func)!);

    /// <summary>Asynchronously creates and stashes a shared value under a type-based key.</summary>
    public static CheckrOf<T> Stashed<T>(Func<Task<T>> func)
        => Stashed<T>(typeof(T).FullName!, _ => func());

    /// <summary>Asynchronously creates and stashes a shared value under a custom key.</summary>
    public static CheckrOf<T> Stashed<T>(string key, Func<Task<T>> func)
        => Stashed<T>(key, _ => func());

    /// <summary>Asynchronously creates and stashes a shared value using the run's cancellation token.</summary>
    public static CheckrOf<T> Stashed<T>(Func<CancellationToken, Task<T>> func)
        => Stashed(typeof(T).FullName!, func);

    /// <summary>Asynchronously creates and stashes a shared value under a custom key using the run's cancellation token.</summary>
    public static CheckrOf<T> Stashed<T>(string key, Func<CancellationToken, Task<T>> func) =>
        async state =>
        {
            state.CancellationToken.ThrowIfCancellationRequested();
            var value = await state.Memory.StoreStashedAsync(key, () => func(state.CancellationToken)).ConfigureAwait(false);
            state.CancellationToken.ThrowIfCancellationRequested();
            return CheckrResult.Some(state, value);
        };
}
