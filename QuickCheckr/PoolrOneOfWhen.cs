using QuickCheckr.UnderTheHood;
using QuickCheckr.UnderTheHood.WhenThingsGoWrong;
using QuickFuzzr;

namespace QuickCheckr;

public static partial class Poolr
{
    /// <summary>Selects a matching pool value, throwing when no stored value matches.</summary>
    public static CheckrOf<T> OneOfWhen<T>(string key, Func<T, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        return OneOfElement(key, predicate).Select(element => element.Value);
    }

    /// <summary>Selects a matching pair of pool values, throwing when no stored pair matches.</summary>
    public static CheckrOf<(T1 First, T2 Second)> OneOfWhen<T1, T2>(
        string key, Func<T1, T2, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        return OneOfPair(key, predicate);
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
