using System.Linq.Expressions;

namespace QuickCheckr.Supplier;

public class SupplrState
{
    private readonly Dictionary<string, Shrinker> shrinkers = [];

    public void Register<T, TProperty>(
        Expression<Func<T, TProperty>> expression,
        ISupplrDefinition<TProperty> definition)
    {
        var shrinker = Shrink.When<TProperty>(
            condition => condition.As(expression).Target(),
            classification => classification.Classify(Try.Function<TProperty>(definition.GetAlternatives)));

        shrinkers.TryAdd($"{typeof(T).FullName}:{expression}", shrinker);
    }

    public Shrinker[] GetShrinkers() => [.. shrinkers.Values];
}
