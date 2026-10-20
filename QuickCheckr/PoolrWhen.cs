using QuickCheckr.UnderTheHood;
using QuickCheckr.UnderTheHood.WhenThingsGoWrong;
using QuickFuzzr;

namespace QuickCheckr;

public static partial class Poolr
{
    /// <summary>Creates a conditional option that selects a pool element when executed.</summary>
    public static CheckrOf<CheckrOption> When<T>(
        string key,
        Func<IReadOnlyCollection<T>, bool> predicate,
        Func<T, CheckrOf<Case>> checkr) =>
            When(predicate).Then(
                from element in OneOfElement<T>(key)
                from result in checkr(element.Value)
                select result);

    /// <summary>Creates a deferred condition over a pool's current contents.</summary>
    public static PoolCondition When<T>(Func<IReadOnlyCollection<T>, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        return new(state => predicate((IReadOnlyCollection<T>)state.Memory.GetPool<T>()));
    }

    public static CheckrOf<CheckrOption> When<T>(
        Func<IReadOnlyCollection<T>, bool> predicate,
        CheckrOf<Case> checkr) =>
            When(predicate).Then(checkr);


    /// <summary>Creates an option that selects a matching pair from two pools when executed.</summary>
    public static CheckrOf<CheckrOption> When<T1, T2>(
        string key,
        Func<T1, T2, bool> predicate,
        Func<T1, T2, CheckrOf<Case>> checkr)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        ArgumentNullException.ThrowIfNull(checkr);
        return When<T1, T2>((first, second) =>
                first.Any(left => second.Any(right => predicate(left, right))))
            .Then(
                from pair in OneOfPair<T1, T2>(key, predicate)
                from result in checkr(pair.First, pair.Second)
                select result);
    }

    /// <summary>Creates a deferred condition over two pools' current contents.</summary>
    public static PoolCondition When<T1, T2>(
        Func<IReadOnlyCollection<T1>, IReadOnlyCollection<T2>, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        return new(state => predicate(
            (IReadOnlyCollection<T1>)state.Memory.GetPool<T1>(),
            (IReadOnlyCollection<T2>)state.Memory.GetPool<T2>()));
    }

    private static CheckrOf<(T1 First, T2 Second)> OneOfPair<T1, T2>(
        string key, Func<T1, T2, bool> predicate) =>
        state =>
        {
            var (id1, id2) = state.Memory.StoreVariable(key, () =>
            {
                var pairs =
                    from first in state.Memory.GetPoolDictionary<T1>()
                    from second in state.Memory.GetPoolDictionary<T2>()
                    where predicate(first.Value, second.Value)
                    select (first.Key, second.Key);
                if (!pairs.Any())
                    throw new NoMatchingPoolElementException(key, typeof(T1), typeof(T2));
                return Fuzzr.OneOf(pairs)(state.FuzzrState).Value;
            });
            state.Memory.StorePoolTrace(key, () => $"( {typeof(T1).Name}-{id1}, {typeof(T2).Name}-{id2} )");
            var (foundFirst, firstValue) = state.Memory.GetFromPool<T1>(id1);
            var (foundSecond, secondValue) = state.Memory.GetFromPool<T2>(id2);
            if (foundFirst && foundSecond && predicate(firstValue!, secondValue!))
                return CheckrResult.Some(state, (firstValue!, secondValue!));

            state.AbortExecution();
            return CheckrResult.Default<(T1, T2)>(state);
        };
}
