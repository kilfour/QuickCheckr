using QuickFuzzr;

namespace QuickCheckr.Supplier;

public static class Supplr
{
    /// <summary>
    /// Describes integers in the range [minimum, maximum), following QuickFuzzr's
    /// minimum-inclusive and maximum-exclusive range semantics.
    /// </summary>
    public static SupplrOf<int> Int(int minimum, int maximum)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(minimum, maximum);

        return new SupplrOf<int>(
            Fuzzr.Int(minimum, maximum),
            value => SimplifyTowardsMinimum(value, minimum));
    }

    private static IEnumerable<int> SimplifyTowardsMinimum(int value, int minimum)
    {
        while (value > minimum)
            yield return --value;
    }
}
