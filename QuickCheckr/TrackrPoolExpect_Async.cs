using QuickCheckr.UnderTheHood;
namespace QuickCheckr;
public static partial class Trackr
{

    public static CheckrOf<Case> PoolExpect<T>(string key, Func<IReadOnlyCollection<T>, Task<bool>> condition)
        => PoolExpect<T>(key, (pool, _) => condition(pool));
    public static CheckrOf<Case> PoolExpect<T>(string key, Func<IReadOnlyCollection<T>, CancellationToken, Task<bool>> condition)
        => state => Checkr.Expect(key, token => condition((IReadOnlyCollection<T>)state.Memory.GetPool<T>(), token))(state);
    public static CheckrOf<Case> PoolExpectEach<T>(string key, Func<T, Task<bool>> expectation)
        => PoolExpectEach<T>(key, (item, _) => expectation(item));
    public static CheckrOf<Case> PoolExpectEach<T>(string key, Func<T, CancellationToken, Task<bool>> expectation)
        => PoolExpectEachAsync<T>(key, (_, _) => Task.FromResult(true), expectation);

    public static CheckrOf<Case> PoolExpectEachWhen<T>(string key, Func<T, bool> predicate, Func<T, Task<bool>> expectation)
        => PoolExpectEachAsync<T>(key, (item, token) => Task.FromResult(predicate(item)), (item, token) => expectation(item));

    public static CheckrOf<Case> PoolExpectEach<T1, T2>(string key, Func<T1, T2, bool> belongsTo,
        Func<T1, IReadOnlyCollection<T2>, Task<bool>> expectation)
        => PoolExpectRelatedAsync<T1, T2>(key, (parent, child, token) => Task.FromResult(belongsTo(parent, child)), (parent, children, token) => expectation(parent, children));

    public static CheckrOf<Case> PoolExpectEachWhen<T>(string key, Func<T, bool> predicate, Func<T, CancellationToken, Task<bool>> expectation)
        => PoolExpectEachAsync<T>(key, (item, token) => Task.FromResult(predicate(item)), expectation);

    public static CheckrOf<Case> PoolExpectEach<T1, T2>(string key, Func<T1, T2, bool> belongsTo,
        Func<T1, IReadOnlyCollection<T2>, CancellationToken, Task<bool>> expectation)
        => PoolExpectRelatedAsync<T1, T2>(key, (parent, child, token) => Task.FromResult(belongsTo(parent, child)), expectation);

    public static CheckrOf<Case> PoolExpectEachWhen<T>(string key, Func<T, Task<bool>> predicate, Func<T, bool> expectation)
        => PoolExpectEachAsync<T>(key, (item, token) => predicate(item), (item, token) => Task.FromResult(expectation(item)));

    public static CheckrOf<Case> PoolExpectEach<T1, T2>(string key, Func<T1, T2, Task<bool>> belongsTo,
        Func<T1, IReadOnlyCollection<T2>, bool> expectation)
        => PoolExpectRelatedAsync<T1, T2>(key, (parent, child, token) => belongsTo(parent, child), (parent, children, token) => Task.FromResult(expectation(parent, children)));

    public static CheckrOf<Case> PoolExpectEachWhen<T>(string key, Func<T, Task<bool>> predicate, Func<T, Task<bool>> expectation)
        => PoolExpectEachAsync<T>(key, (item, token) => predicate(item), (item, token) => expectation(item));

    public static CheckrOf<Case> PoolExpectEach<T1, T2>(string key, Func<T1, T2, Task<bool>> belongsTo,
        Func<T1, IReadOnlyCollection<T2>, Task<bool>> expectation)
        => PoolExpectRelatedAsync<T1, T2>(key, (parent, child, token) => belongsTo(parent, child), (parent, children, token) => expectation(parent, children));

    public static CheckrOf<Case> PoolExpectEachWhen<T>(string key, Func<T, Task<bool>> predicate, Func<T, CancellationToken, Task<bool>> expectation)
        => PoolExpectEachAsync<T>(key, (item, token) => predicate(item), expectation);

    public static CheckrOf<Case> PoolExpectEach<T1, T2>(string key, Func<T1, T2, Task<bool>> belongsTo,
        Func<T1, IReadOnlyCollection<T2>, CancellationToken, Task<bool>> expectation)
        => PoolExpectRelatedAsync<T1, T2>(key, (parent, child, token) => belongsTo(parent, child), expectation);

    public static CheckrOf<Case> PoolExpectEachWhen<T>(string key, Func<T, CancellationToken, Task<bool>> predicate, Func<T, bool> expectation)
        => PoolExpectEachAsync<T>(key, predicate, (item, token) => Task.FromResult(expectation(item)));

    public static CheckrOf<Case> PoolExpectEach<T1, T2>(string key, Func<T1, T2, CancellationToken, Task<bool>> belongsTo,
        Func<T1, IReadOnlyCollection<T2>, bool> expectation)
        => PoolExpectRelatedAsync<T1, T2>(key, belongsTo, (parent, children, token) => Task.FromResult(expectation(parent, children)));

    public static CheckrOf<Case> PoolExpectEachWhen<T>(string key, Func<T, CancellationToken, Task<bool>> predicate, Func<T, Task<bool>> expectation)
        => PoolExpectEachAsync<T>(key, predicate, (item, token) => expectation(item));

    public static CheckrOf<Case> PoolExpectEach<T1, T2>(string key, Func<T1, T2, CancellationToken, Task<bool>> belongsTo,
        Func<T1, IReadOnlyCollection<T2>, Task<bool>> expectation)
        => PoolExpectRelatedAsync<T1, T2>(key, belongsTo, (parent, children, token) => expectation(parent, children));

    public static CheckrOf<Case> PoolExpectEachWhen<T>(string key, Func<T, CancellationToken, Task<bool>> predicate, Func<T, CancellationToken, Task<bool>> expectation)
        => PoolExpectEachAsync<T>(key, predicate, expectation);

    public static CheckrOf<Case> PoolExpectEach<T1, T2>(string key, Func<T1, T2, CancellationToken, Task<bool>> belongsTo,
        Func<T1, IReadOnlyCollection<T2>, CancellationToken, Task<bool>> expectation)
        => PoolExpectRelatedAsync<T1, T2>(key, belongsTo, expectation);

    private static CheckrOf<Case> PoolExpectEachAsync<T>(string key,
        Func<T, CancellationToken, Task<bool>> predicate,
        Func<T, CancellationToken, Task<bool>> expectation) =>
        state =>
        {
            var candidates = state.Memory.GetPoolDictionary<T>().ToList();
            if (candidates.Count == 0) return CheckrResult.CaseOnly(state);
            List<KeyValuePair<int, T>> selected = [];
            List<string> failed = [];
            return Evaluate.ExpectAsync(key, async token =>
            {
                foreach (var item in selected)
                {
                    token.ThrowIfCancellationRequested();
                    var passed = await expectation(item.Value, token).ConfigureAwait(false);
                    token.ThrowIfCancellationRequested();
                    if (!passed) failed.Add($"{typeof(T).Name}-{item.Key}");
                }
                return failed.Count == 0;
            }, true, () => failed, async token =>
            {
                // Filter callbacks run inside the expectation's skip, exception and cancellation guards.
                foreach (var item in candidates)
                {
                    token.ThrowIfCancellationRequested();
                    var include = await predicate(item.Value, token).ConfigureAwait(false);
                    token.ThrowIfCancellationRequested();
                    if (include) selected.Add(item);
                }
                return selected.Count != 0;
            })(state);
        };

    private static CheckrOf<Case> PoolExpectRelatedAsync<T1, T2>(string key,
        Func<T1, T2, CancellationToken, Task<bool>> belongsTo,
        Func<T1, IReadOnlyCollection<T2>, CancellationToken, Task<bool>> expectation) =>
        state =>
        {
            var parents = state.Memory.GetPoolDictionary<T1>().ToList();
            var other = state.Memory.GetPool<T2>().ToList();
            return Combine.Checkrs(parents.Select(parent =>
                Checkr.Expect($"{key}: {typeof(T1).Name}-{parent.Key}", async token =>
                {
                    List<T2> children = [];
                    foreach (var child in other)
                    {
                        token.ThrowIfCancellationRequested();
                        var include = await belongsTo(parent.Value, child, token).ConfigureAwait(false);
                        token.ThrowIfCancellationRequested();
                        if (include) children.Add(child);
                    }
                    return await expectation(parent.Value, children, token).ConfigureAwait(false);
                })))(state);
        };
}
