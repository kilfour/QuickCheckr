using QuickFuzzr;

namespace QuickCheckr.Supplier;

/// <summary>
/// Describes how values in a valid domain are generated and simplified.
/// </summary>
public sealed class SupplrOf<T>
{
    internal SupplrOf(
        FuzzrOf<T> fuzzr,
        Func<T, IEnumerable<T>> simplifications)
    {
        Fuzzr = fuzzr;
        Simplifications = simplifications;
    }

    /// <summary>
    /// Interprets this supply description as a QuickFuzzr generator.
    /// </summary>
    public FuzzrOf<T> Fuzzr { get; }

    /// <summary>
    /// Produces progressively simpler values that remain inside the described domain.
    /// </summary>
    public Func<T, IEnumerable<T>> Simplifications { get; }
}
