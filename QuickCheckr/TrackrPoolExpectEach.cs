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
            var expect = Evaluate.Expectation(
                key,
                _ =>
                {
                    string[] failed = [.. candidates
                        .Where(a => !expectation(a.Value))
                        .Select(a => $"{typeof(T).Name}-{a.Key}")];
                    return failed.Length == 0 ? ExpectationResult.Passed() : ExpectationResult.Failed(failed);
                },
                true);
            return expect(state);
        };

    public static CheckrOf<Case> PoolExpectEach<T1, T2>(string key,
        Func<T1, T2, bool> belongsTo, Func<T1, IReadOnlyCollection<T2>, bool> expectation)
        => PoolExpectRelatedAsync<T1, T2>(key,
            (parent, child, _) => Task.FromResult(belongsTo(parent, child)),
            (parent, children, _) => Task.FromResult(expectation(parent, children)));
}