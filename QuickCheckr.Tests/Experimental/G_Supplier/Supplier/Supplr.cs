namespace QuickCheckr.Supplier;

public static partial class Supplr
{
    public static SupplrOf<int> Int(int min, int max)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(min, max);
        return s => new SupplrResult<int>(s, new IntRange(min, max));
    }
}
