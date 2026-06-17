using QuickCheckr.UnderTheHood;

namespace QuickCheckr;

public static partial class Trackr
{
    // public static CheckrOf<Case> PoolExpectEach<T>(
    //     string key,
    //     Func<T, bool> expectation) =>
    //     state =>
    //     {
    //         var candidates = state.Memory.GetPoolDictionary<T>();
    //         if (candidates.Count == 0)
    //             return CheckrResult.CaseOnly(state);
    //         var failed = candidates.Where(a => !expectation(a.Value));
    //         var expect = Evaluate.Expect(key, () => !failed.Any(), true, () => [.. failed.Select(a => $"{typeof(T).Name}-{a.Key}")]);
    //         return expect(state);
    //     };
    // state => Combine.Checkrs(
    //     state.Memory.GetPoolDictionary<T>()
    //         .Select(a => Checkr.Expect($"{key}: {typeof(T).Name}-{a.Key}", () => expectation(a.Value))))(state);

    public static CheckrOf<Case> PoolExpectEach<T>(
        string key,
        Func<T, bool> expectation) =>
        state =>
        {
            using var _ = state.Pathologist.Examine("PoolExpectEach");
            var candidates = state.Memory.GetPoolDictionary<T>().ToList();
            if (candidates.Count == 0)
                return CheckrResult.CaseOnly(state);
            state.Pathologist.Inform(string.Join("/", candidates.Select(a => a.Key)));
            List<string> failed = [];
            var expect = Evaluate.Expect(
                key,
                () =>
                {
                    failed = [.. candidates
                    .Where(a => !expectation(a.Value))
                    .Select(a => $"{typeof(T).Name}-{a.Key}")];
                    // state.Pathologist.Inform("Message", string.Join("|", failed));
                    return failed.Count == 0;
                },
                true,
                () => failed);
            return expect(state);
        };

    public static CheckrOf<Case> PoolExpectEach<T1, T2>(
        string key,
        Func<T1, T2, bool> belongsTo,
        Func<T1, IReadOnlyCollection<T2>, bool> expectation) =>
        state =>
        {
            var poolDictionary = state.Memory.GetPoolDictionary<T1>();
            var other = state.Memory.GetPool<T2>();
            var combined = poolDictionary.Select(a => (KeyValueElement: a, Pool: other.Where(b => belongsTo(a.Value, b))));
            IEnumerable<CheckrOf<Case>> checkrs =
                combined
                    .Select(a =>
                        Checkr.Expect($"{key}: {typeof(T1).Name}-{a.KeyValueElement.Key}",
                            () => expectation(a.KeyValueElement.Value, [.. a.Pool])));
            // return Checkr.Expect(key, () => combined.All(a => expectation(a.KeyValueElement.Value, [.. a.Pool])))(state);
            return Combine.Checkrs(checkrs)(state);
        };
    //  public static CheckrOf<Case> PoolExpectEach<T1, T2>(
    //     string key,
    //     Func<T1, T2, bool> belongsTo,
    //     Func<T1, IReadOnlyCollection<T2>, bool> expectation) =>
    //     state =>
    //     {
    //         var poolDictionary = state.Memory.GetPoolDictionary<T1>().ToList();
    //         var other = state.Memory.GetPool<T2>().ToList();

    //         var checkrs =
    //             poolDictionary.Select(a =>
    //             {
    //                 var poolKey = a.Key;
    //                 var value = a.Value;

    //                 var related = other
    //                     .Where(b => belongsTo(value, b))
    //                     .ToList();

    //                 return Checkr.Expect(
    //                     $"{key}: {typeof(T1).Name}-{poolKey}",
    //                     () => expectation(value, related));
    //             });

    //         return Combine.Checkrs(checkrs)(state);
    //     };
}