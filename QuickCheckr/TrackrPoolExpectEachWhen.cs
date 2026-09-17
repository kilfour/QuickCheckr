namespace QuickCheckr;

public static partial class Trackr
{
    public static CheckrOf<Case> PoolExpectEachWhen<T>(string key, Func<T, bool> predicate, Func<T, bool> condition)
        => PoolExpectEachAsync<T>(key,
            (item, _) => Task.FromResult(predicate(item)),
            (item, _) => Task.FromResult(condition(item)));
}
