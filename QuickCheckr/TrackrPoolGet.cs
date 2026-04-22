using QuickCheckr.UnderTheHood;

namespace QuickCheckr;

public static partial class Trackr
{
    public static CheckrOf<IReadOnlyCollection<T>> GetPool<T>() =>
        state =>
        {
            state.Memory.ClearPool<T>();
            return CheckrResult.Some(state, (IReadOnlyCollection<T>)state.Memory.GetPool<T>());
        };
}
