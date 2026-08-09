using QuickCheckr.Supplier;
using QuickFuzzr;

namespace QuickCheckr;

public static partial class Checkr
{
    /// <summary>
    /// Creates an input from a domain-aware supplier. Generation and shrinking are
    /// both derived from the supplier's description of valid values.
    /// </summary>
    public static CheckrOf<T> Input<T>(string key, SupplrOf<T> supplr)
    {
        ArgumentNullException.ThrowIfNull(supplr);

        var definition = supplr.Resolve();
        return state =>
        {
            var result = HandleInput(state, key, definition.GetFuzzr());
            state.Memory.StoreShrinkers(key, definition.GetShrinkers());
            return result;
        };
    }

    public static CheckrOf<T> Input<T>(string key, SupplrOf<Intent> supplr)
    {
        ArgumentNullException.ThrowIfNull(supplr);

        var definition = supplr.Resolve();
        return state =>
        {
            var result = HandleInput(state, key,
                from _ in definition.GetFuzzr()
                from one in Fuzzr.One<T>()
                select one);
            state.Memory.StoreShrinkers(key, definition.GetShrinkers());
            return result;
        };
    }
}
