using QuickCheckr.UnderTheHood;

namespace QuickCheckr;

public static partial class Trackr
{
    /// <summary>
    /// Creates a new pool for values of the given type and returns it.  
    /// Use when you want to start collecting objects across executions within a run.
    /// </summary>
    public static CheckrOf<Pool<T>> PoolFor<T>() =>
        state => CheckrResult.Some(state, new Pool<T>(state.Memory.CreatePool<T>()));
}
