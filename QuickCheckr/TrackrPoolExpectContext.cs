using QuickCheckr.UnderTheHood;

namespace QuickCheckr;

public static partial class Trackr
{
    public static CheckrOf<Case> PoolExpectEach<T>(string key,
        Func<T, ExpectationContext, ExpectationResult> expectation)
        => PoolExpectEachWhen<T>(key, _ => true, expectation);

    public static CheckrOf<Case> PoolExpectEachWhen<T>(string key, Func<T, bool> predicate,
        Func<T, ExpectationContext, ExpectationResult> expectation)
        => PoolExpectEachAsync<T>(key,
            (item, _) => Task.FromResult(predicate(item)),
            (item, context, _) => Task.FromResult(expectation(item, context)));

    public static CheckrOf<Case> PoolExpectEach<T>(string key,
        Func<T, ExpectationContext, Task<ExpectationResult>> expectation)
        => PoolExpectEach<T>(key, (item, context, _) => expectation(item, context));

    public static CheckrOf<Case> PoolExpectEach<T>(string key,
        Func<T, ExpectationContext, CancellationToken, Task<ExpectationResult>> expectation)
        => PoolExpectEachAsync<T>(key, (_, _) => Task.FromResult(true), expectation);
    public static CheckrOf<Case> PoolExpectEachWhen<T>(string key, Func<T, bool> predicate,
        Func<T, ExpectationContext, Task<ExpectationResult>> expectation)
        => PoolExpectEachAsync<T>(key, (item, _) => Task.FromResult(predicate(item)), (item, context, _) => expectation(item, context));

    public static CheckrOf<Case> PoolExpectEachWhen<T>(string key, Func<T, bool> predicate,
        Func<T, ExpectationContext, CancellationToken, Task<ExpectationResult>> expectation)
        => PoolExpectEachAsync<T>(key, (item, _) => Task.FromResult(predicate(item)), expectation);

    public static CheckrOf<Case> PoolExpectEachWhen<T>(string key, Func<T, Task<bool>> predicate,
        Func<T, ExpectationContext, ExpectationResult> expectation)
        => PoolExpectEachAsync<T>(key, (item, _) => predicate(item), (item, context, _) => Task.FromResult(expectation(item, context)));

    public static CheckrOf<Case> PoolExpectEachWhen<T>(string key, Func<T, Task<bool>> predicate,
        Func<T, ExpectationContext, Task<ExpectationResult>> expectation)
        => PoolExpectEachAsync<T>(key, (item, _) => predicate(item), (item, context, _) => expectation(item, context));

    public static CheckrOf<Case> PoolExpectEachWhen<T>(string key, Func<T, Task<bool>> predicate,
        Func<T, ExpectationContext, CancellationToken, Task<ExpectationResult>> expectation)
        => PoolExpectEachAsync<T>(key, (item, _) => predicate(item), expectation);

    public static CheckrOf<Case> PoolExpectEachWhen<T>(string key, Func<T, CancellationToken, Task<bool>> predicate,
        Func<T, ExpectationContext, ExpectationResult> expectation)
        => PoolExpectEachAsync<T>(key, predicate, (item, context, _) => Task.FromResult(expectation(item, context)));

    public static CheckrOf<Case> PoolExpectEachWhen<T>(string key, Func<T, CancellationToken, Task<bool>> predicate,
        Func<T, ExpectationContext, Task<ExpectationResult>> expectation)
        => PoolExpectEachAsync<T>(key, predicate, (item, context, _) => expectation(item, context));

    public static CheckrOf<Case> PoolExpectEachWhen<T>(string key, Func<T, CancellationToken, Task<bool>> predicate,
        Func<T, ExpectationContext, CancellationToken, Task<ExpectationResult>> expectation)
        => PoolExpectEachAsync<T>(key, predicate, expectation);

    public static CheckrOf<Case> PoolExpectEach<T1, T2>(string key, Func<T1, T2, bool> belongsTo,
        Func<T1, IReadOnlyCollection<T2>, ExpectationContext, ExpectationResult> expectation)
        => PoolExpectRelatedAsync<T1, T2>(key, (parent, child, _) => Task.FromResult(belongsTo(parent, child)), (parent, children, context, _) => Task.FromResult(expectation(parent, children, context)));

    public static CheckrOf<Case> PoolExpectEach<T1, T2>(string key, Func<T1, T2, bool> belongsTo,
        Func<T1, IReadOnlyCollection<T2>, ExpectationContext, Task<ExpectationResult>> expectation)
        => PoolExpectRelatedAsync<T1, T2>(key, (parent, child, _) => Task.FromResult(belongsTo(parent, child)), (parent, children, context, _) => expectation(parent, children, context));

    public static CheckrOf<Case> PoolExpectEach<T1, T2>(string key, Func<T1, T2, bool> belongsTo,
        Func<T1, IReadOnlyCollection<T2>, ExpectationContext, CancellationToken, Task<ExpectationResult>> expectation)
        => PoolExpectRelatedAsync<T1, T2>(key, (parent, child, _) => Task.FromResult(belongsTo(parent, child)), expectation);

    public static CheckrOf<Case> PoolExpectEach<T1, T2>(string key, Func<T1, T2, Task<bool>> belongsTo,
        Func<T1, IReadOnlyCollection<T2>, ExpectationContext, ExpectationResult> expectation)
        => PoolExpectRelatedAsync<T1, T2>(key, (parent, child, _) => belongsTo(parent, child), (parent, children, context, _) => Task.FromResult(expectation(parent, children, context)));

    public static CheckrOf<Case> PoolExpectEach<T1, T2>(string key, Func<T1, T2, Task<bool>> belongsTo,
        Func<T1, IReadOnlyCollection<T2>, ExpectationContext, Task<ExpectationResult>> expectation)
        => PoolExpectRelatedAsync<T1, T2>(key, (parent, child, _) => belongsTo(parent, child), (parent, children, context, _) => expectation(parent, children, context));

    public static CheckrOf<Case> PoolExpectEach<T1, T2>(string key, Func<T1, T2, Task<bool>> belongsTo,
        Func<T1, IReadOnlyCollection<T2>, ExpectationContext, CancellationToken, Task<ExpectationResult>> expectation)
        => PoolExpectRelatedAsync<T1, T2>(key, (parent, child, _) => belongsTo(parent, child), expectation);

    public static CheckrOf<Case> PoolExpectEach<T1, T2>(string key, Func<T1, T2, CancellationToken, Task<bool>> belongsTo,
        Func<T1, IReadOnlyCollection<T2>, ExpectationContext, ExpectationResult> expectation)
        => PoolExpectRelatedAsync<T1, T2>(key, belongsTo, (parent, children, context, _) => Task.FromResult(expectation(parent, children, context)));

    public static CheckrOf<Case> PoolExpectEach<T1, T2>(string key, Func<T1, T2, CancellationToken, Task<bool>> belongsTo,
        Func<T1, IReadOnlyCollection<T2>, ExpectationContext, Task<ExpectationResult>> expectation)
        => PoolExpectRelatedAsync<T1, T2>(key, belongsTo, (parent, children, context, _) => expectation(parent, children, context));

    public static CheckrOf<Case> PoolExpectEach<T1, T2>(string key, Func<T1, T2, CancellationToken, Task<bool>> belongsTo,
        Func<T1, IReadOnlyCollection<T2>, ExpectationContext, CancellationToken, Task<ExpectationResult>> expectation)
        => PoolExpectRelatedAsync<T1, T2>(key, belongsTo, expectation);

}
