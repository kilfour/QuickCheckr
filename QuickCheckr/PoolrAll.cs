using QuickCheckr.UnderTheHood;

namespace QuickCheckr;

public static partial class Poolr
{
    public static CheckrOf<IReadOnlyCollection<T>> All<T>() =>
        state => CheckrResult.Some(state, (IReadOnlyCollection<T>)state.Memory.GetPool<T>());
}
