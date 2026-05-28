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
            var candidates = state.Memory.GetPoolDictionary<T>().Where(a => predicate(a.Value));
            if (!candidates.Any())
                return CheckrResult.CaseOnly(state);
            var failed = candidates.Where(a => !condition(a.Value));
            var expect = Evaluate.Expect(key, () => !failed.Any(), true, () => [.. failed.Select(a => $"{typeof(T).Name}-{a.Key}")]);
            return expect(state);
        };
    // return Combine.Checkrs(
    //     state.Memory.GetPoolDictionary<T>()
    //         .Where(a => predicate(a.Value))
    //         .Select(a => Checkr.Expect($"{key}: {typeof(T).Name}-{a.Key}", () => condition(a.Value))))(state);
    // return Checkr.Expect(key, () => candidates.All(a => condition(a)))(state);
}