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

    /// <summary>
    /// Retrieves a stashed value using the type name as key, creating and storing it on first use.  
    /// Use when you want a convenient shorthand for a single stashed instance per type.
    /// </summary>
    public static CheckrOf<T> Stashed<T>()
        => Stashed<T>(typeof(T).FullName!);

    /// <summary>
    /// Retrieves a stashed value using the specified key.  
    /// Use when you previously stored a value with <see cref="Stashed{T}(string, Func{T})"/> and want to reuse it later in the script.
    /// </summary>
    public static CheckrOf<T> Stashed<T>(string key) =>
       state => CheckrResult.Some(state, state.Memory.GetStashed<T>(key));

}