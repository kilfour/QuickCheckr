using QuickCheckr.UnderTheHood;

namespace QuickCheckr;

public static partial class Trackr
{
    public static CheckrOf<Case> PoolExpectEach<T>(
        string key,
        Func<T, bool> condition) =>
        state => Combine.Checkrs(
            state.Memory.GetPoolDictionary<T>()
                .Select(a => Checkr.Expect($"{key}: {typeof(T).Name}-{a.Key}", () => condition(a.Value))))(state);
}