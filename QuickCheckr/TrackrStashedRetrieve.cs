using QuickCheckr.UnderTheHood;

namespace QuickCheckr;

public static partial class Trackr
{
    /// <summary>
    /// Retrieves a stashed value using the type name as key, creating and storing it on first use.  
    /// Use when you want a convenient shorthand for a single stashed instance per type.
    /// </summary>
    public static CheckrOf<T> GetStashed<T>()
        => GetStashed<T>(typeof(T).FullName!);

    /// <summary>
    /// Retrieves a stashed value using the specified key.  
    /// Use when you previously stored a value with <see cref="Stashed{T}(string, Func{T})"/> and want to reuse it later in the script.
    /// </summary>
    public static CheckrOf<T> GetStashed<T>(string key) =>
       state => CheckrResult.Some(state, state.Memory.GetStashed<T>(key));
}