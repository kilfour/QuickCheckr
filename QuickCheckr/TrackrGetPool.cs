using QuickCheckr.UnderTheHood;
using QuickFuzzr;

namespace QuickCheckr;

public static partial class Trackr
{
    /// <summary>
    /// Retrieves the existing pool for values of the given type.  
    /// Use to observe or draw from objects accumulated earlier in the run.
    /// </summary>
    public static CheckrOf<Pool<T>> GetPool<T>() =>
        state => CheckrResult.Some(state, new Pool<T>(state.Memory.GetPool<T>()));
}
