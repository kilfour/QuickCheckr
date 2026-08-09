using System.Linq.Expressions;
using QuickFuzzr;

namespace QuickCheckr.Supplier;

public static partial class Supplr<T>
{
    public static SupplrOf<Intent> Property<TProperty>(
        Expression<Func<T, TProperty>> expression,
        SupplrOf<TProperty> supplr
    )
    {
        ArgumentNullException.ThrowIfNull(expression);
        ArgumentNullException.ThrowIfNull(supplr);

        return state =>
        {
            var propertyResult = supplr(state);
            propertyResult.State.Register(expression, propertyResult.Definition);
            var definition = new DerivedDefinition<Intent>(
                () => Configr<T>
                    .Property(expression, propertyResult.Definition.GetFuzzr()),
                propertyResult.State.GetShrinkers);
            return SupplrResult.Some(propertyResult.State, definition);
        };
    }
}
