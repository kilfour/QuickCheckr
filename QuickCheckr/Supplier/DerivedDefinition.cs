using QuickFuzzr;

namespace QuickCheckr.Supplier;

internal sealed class DerivedDefinition<T>(
    Func<FuzzrOf<T>> fuzzrFactory,
    Func<Shrinker[]>? shrinkerFactory = null)
    : ISupplrDefinition<T>
{
    public FuzzrOf<T> GetFuzzr() => fuzzrFactory();

    public Shrinker[] GetShrinkers() => shrinkerFactory?.Invoke() ?? [];

    public IEnumerable<T> GetAlternatives(T original) => [];
}
