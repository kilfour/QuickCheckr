namespace QuickCheckr;

public class Pool<T>(IEnumerable<T> pool)
{
    public IEnumerable<T> Values => pool;

    // public CheckrOf<(Func<bool> condition, CheckrOf<Case> checkr)> When(
    //     Func<IEnumerable<T>, bool> predicate,
    //     CheckrOf<Case> checkr)
    //     => Checkr.Option(() => predicate(pool), checkr);

    // public CheckrOf<(Func<bool> condition, CheckrOf<Case> checkr)> When(
    //     Func<IEnumerable<T>, bool> predicate,
    //     string key,
    //     Func<PoolElement<T>, CheckrOf<Case>> checkr)
    //     => Checkr.Option(() => predicate(pool), Trackr.OneOfPool(key, checkr));

    public CheckrOf<Case> Expect(
        string key,
        Func<IEnumerable<T>, bool> condition)
            => Checkr.Expect(key, () => condition(pool));

    public CheckrOf<Case> ExpectForEach(
        string key,
        Func<T, bool> condition)
            => Checkr.Expect(key, () => pool.All(a => condition(a)));

    public CheckrOf<Case> ExpectForEachWhen(
        string key,
        Func<T, bool> predicate,
        Func<T, bool> condition)
        => Checkr.Expect(key, () => pool.Where(a => predicate(a)).All(a => condition(a)));
}

public class Pool<T1, T2>(IEnumerable<T1> pool1, IEnumerable<T2> pool2)
{
    private bool HasAtleastOne(Func<T1, T2, bool> predicate) =>
        pool1.SelectMany(
            t1 => pool2.Select(t2 => predicate(t1, t2)))
            .All(a => a);

    public CheckrOf<(Func<bool> condition, CheckrOf<Case> checkr)> When(
        string key,
        Func<T1, T2, bool> predicate,
        Func<T1, T2, CheckrOf<Case>> checkr)
        => Checkr.Option(() => HasAtleastOne(predicate), Trackr.OneOfPoolWhen(key, predicate, checkr));
}
