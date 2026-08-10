using QuickFuzzr;

namespace QuickCheckr.Supplier;

internal sealed class DerivedDefinition<T>(
    Func<FuzzrOf<T>> fuzzrFactory,
    Func<Shrinker[]>? shrinkerFactory = null)
    : ISupplrDefinition<T>
{
    public FuzzrOf<T> GetFuzzr() => fuzzrFactory();

    public Shrinker[] GetShrinkers()
    {
        var shrinkers = shrinkerFactory?.Invoke() ?? [];
        return shrinkers.Length != 0
            ? shrinkers
            : [Shrink.OnType<T>(classification =>
                classification.Classify(Try.Fuzzr(GetFuzzr())))];
    }

    public IEnumerable<T> GetAlternatives(T original) => [];
}
