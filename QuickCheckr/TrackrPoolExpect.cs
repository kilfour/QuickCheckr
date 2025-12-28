namespace QuickCheckr;

public static partial class Trackr
{
    public static CheckrOf<Case> PoolExpect<T>(string key, Func<IReadOnlyCollection<T>, bool> condition) =>
        state => Checkr.Expect(key, () => condition((IReadOnlyCollection<T>)state.Memory.GetPool<T>()))(state);
}