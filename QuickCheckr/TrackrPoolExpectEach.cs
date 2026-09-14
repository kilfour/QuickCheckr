using QuickCheckr.UnderTheHood;

namespace QuickCheckr;

public static partial class Trackr
{
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
            var combined = poolDictionary.Select(a => (KeyValueElement: a, Pool: other.Where(b => belongsTo(a.Value, b)).ToList()));
            IEnumerable<CheckrOf<Case>> checkrs =
                combined
                    .Select(a =>
                        Checkr.Expect($"{key}: {typeof(T1).Name}-{a.KeyValueElement.Key}",
                            () => expectation(a.KeyValueElement.Value, [.. a.Pool])));
            return Combine.Checkrs(checkrs)(state);
        };
}