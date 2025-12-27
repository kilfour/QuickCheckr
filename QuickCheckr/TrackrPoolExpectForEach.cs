using QuickCheckr.UnderTheHood;

namespace QuickCheckr;

public static partial class Trackr
{
    public static CheckrOf<Case> PoolExpectForEach<T>(
        string key,
        Func<T, bool> condition) =>
        state => Checkr.Expect(key, () => state.Memory.GetPool<T>().All(a => condition(a)))(state);
}