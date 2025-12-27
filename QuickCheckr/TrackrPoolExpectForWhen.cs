namespace QuickCheckr;

public static partial class Trackr
{
    public static CheckrOf<Case> PoolExpectEachWhen<T>(
        string key,
        Func<T, bool> predicate,
        Func<T, bool> condition) =>
        state => Checkr.Expect(key, () => state.Memory.GetPool<T>().Where(a => predicate(a)).All(a => condition(a)))(state);
}