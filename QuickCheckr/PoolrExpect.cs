namespace QuickCheckr;

public static partial class Poolr
{
    public static CheckrOf<Case> Expect<T>(
        string key,
        Func<IReadOnlyCollection<T>, ExpectationContext, ExpectationResult> condition) =>
        state => Checkr.Expect(key, context =>
            condition((IReadOnlyCollection<T>)state.Memory.GetPool<T>(), context))(state);

    public static CheckrOf<Case> Expect<T>(string key, Func<IReadOnlyCollection<T>, bool> condition) =>
        state => Checkr.Expect(key, () => condition((IReadOnlyCollection<T>)state.Memory.GetPool<T>()))(state);
}
