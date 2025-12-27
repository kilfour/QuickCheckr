using QuickCheckr.UnderTheHood;

namespace QuickCheckr;

public static partial class Trackr
{

    // public static CheckrOf<Pool<T>> Whens<T>() =>
    //     state => CheckrResult.Some(state, new Pool<T>(state.Memory.CreatePool<T>()));

    // public static (Func<bool>, CheckrOf<Case>) When<T>(
    //     Func<IEnumerable<T>, bool> predicate,
    //     CheckrOf<Case> checkr)

    //     => (() => predicate(pool), checkr);

    // public (Func<bool>, CheckrOf<Case>) When(
    //     Func<IEnumerable<T>, bool> predicate,
    //     string key,
    //     Func<PoolElement<T>, CheckrOf<Case>> checkr)
    //     => (() => predicate(pool), Trackr.OneOfPool(key, checkr));
}