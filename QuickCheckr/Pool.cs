namespace QuickCheckr;

public static class Pool
{
    public static Pool<T1, T2> These<T1, T2>(Pool<T1> pool1, Pool<T2> pool2)
        => new(pool1.Values, pool2.Values);
}


public class Pool<T>(IEnumerable<T> pool)
{
    public IEnumerable<T> Values => pool;

    public (Func<bool>, CheckrOf<Case>) When(
        Func<IEnumerable<T>, bool> predicate,
        CheckrOf<Case> checkr)
        => (() => predicate(pool), checkr);

    public (Func<bool>, CheckrOf<Case>) When(
        Func<IEnumerable<T>, bool> predicate,
        string key,
        Func<T, CheckrOf<Case>> checkr,
        Action<TrackrPoolOperations<T>>? operations = null)
        => (() => predicate(pool), Trackr.OneOfPool(key, checkr, operations));

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

    public (Func<bool>, CheckrOf<Case>) When(
        string key,
        Func<T1, T2, bool> predicate,
        Func<T1, T2, CheckrOf<Case>> checkr)
        => (() => HasAtleastOne(predicate), Trackr.OneOfPool(key, predicate, checkr));
}
