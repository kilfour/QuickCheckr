using QuickCheckr.UnderTheHood;

namespace QuickCheckr;

public static partial class Trackr
{
    public static CheckrOf<T> Stashed<T>(Func<T> func)
        => Stashed(typeof(T).FullName!, func);

    public static CheckrOf<T> Stashed<T>(string key, Func<T> func) =>
        state => CheckrResult.Some(state, state.LongTermMemory.StoreStashed(key, func)!);

    public static CheckrOf<T> Stashed<T>()
        => Stashed<T>(typeof(T).FullName!);

    public static CheckrOf<T> Stashed<T>(string key) =>
       state => CheckrResult.Some(state, state.LongTermMemory.GetStashed<T>(key));



}