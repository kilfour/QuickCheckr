using QuickCheckr.UnderTheHood;

namespace QuickCheckr;

public static partial class Trackr
{
    public static CheckrOf<Case> PoolExpect<T>(string key, Func<IEnumerable<T>, bool> condition) =>
        state => Checkr.Expect(key, () => condition(state.Memory.GetPool<T>()))(state);
}