using QuickCheckr.UnderTheHood;

namespace QuickCheckr;

public static partial class Trackr
{
    public static CheckrOf<T> Stashed<T>(Func<T> func) =>
        state => CheckrResult.Some(state, state.LongTermMemory.Store(typeof(T).FullName!, func)!);
}