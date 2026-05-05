using QuickCheckr.UnderTheHood;

namespace QuickCheckr;

public static partial class Trackr
{
    /// <summary>
    /// Creates a new pool for values of the given type and returns it.  
    /// Use when you want to start collecting objects across executions within a run.
    /// </summary>
    // public static CheckrOf<Case> PoolFor<T>() =>
    //     state =>
    //     {
    //         state.Memory.CreatePool<T>();
    //         return CheckrResult.CaseOnly(state);
    //     };
}
