using QuickCheckr.Supplier;

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

        var shrinker = Shrink.OnType<T>(
            classification => classification.Classify(
                Try.Function(supplr.Simplifications)),
            reduction => reduction.Simplify(
                Reduce.Function(supplr.Simplifications)));

        return Input(key, supplr.Fuzzr, shrinker);
    }
}
