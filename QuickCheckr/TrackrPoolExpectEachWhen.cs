using QuickCheckr.UnderTheHood;

namespace QuickCheckr;

public static partial class Trackr
{
    public static CheckrOf<Case> PoolExpectEachWhen<T>(
        string key,
        Func<T, bool> predicate,
        Func<T, bool> condition) =>
        state =>
        {
            var candidates = state.Memory.GetPool<T>().Where(a => predicate(a));
            if (!candidates.Any())
                return CheckrResult.CaseOnly(state);
            return Combine.Checkrs(
                state.Memory.GetPoolDictionary<T>()
                    .Where(a => predicate(a.Value))
                    .Select(a => Checkr.Expect($"{key}: {typeof(T).Name}-{a.Key}", () => condition(a.Value))))(state);
            // return Checkr.Expect(key, () => candidates.All(a => condition(a)))(state);
        };
}