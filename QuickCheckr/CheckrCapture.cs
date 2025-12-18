using QuickCheckr.UnderTheHood;

namespace QuickCheckr;

public static partial class Checkr
{
    /// <summary>
    /// Captures the value produced by a function without adding an action to the report.  
    /// Use when you want to compute state or helper values that participate in the script but do not represent observable behaviour.
    /// </summary>
    public static CheckrOf<T> Capture<T>(Func<T> func) =>
        state => TheCatcher.InTheCheckr(state, $"Capture: {typeof(T).Name}", () => CheckrResult.Some(state, func()));


    /// <summary>
    /// Captures the value produced by an asynchronous function without adding an action to the report.  
    /// Use when awaiting a helper computation that should influence the script but not appear as an action.
    /// </summary>
    public static CheckrOf<T> Capture<T>(Task<T> func) =>
        state => TheCatcher.InTheCheckr(state, $"Capture: {typeof(T).Name}", () => CheckrResult.Some(state, func.GetAwaiter().GetResult()));
}