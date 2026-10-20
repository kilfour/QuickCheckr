using QuickCheckr.UnderTheHood;
using QuickFuzzr;

namespace QuickCheckr;

public static partial class Poolr
{
    public static CheckrOf<IReadOnlyCollection<PoolElement<T>>> AllElements<T>() =>
        state =>
            CheckrResult.Some(state,
                (IReadOnlyCollection<PoolElement<T>>)
                [.. state.Memory.GetPoolDictionary<T>().Select(a => new PoolElement<T>(a.Key, a.Value))]);
}
