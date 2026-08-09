using QuickFuzzr;
using QuickFuzzr.UnderTheHood;

namespace QuickCheckr.Supplier;

public static partial class Supplr
{
    public static SupplrOf<int> Int(int min, int max)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(min, max);
        return s => new SupplrResult<int>(s, new IntRange(min, max));
    }

    // public static SupplrOf<T> One<T>()
    // {
    //     return state => SupplrResult.Some(
    //         state,
    //         new DerivedDefinition<T>(
    //             () => s => (IResult<T>)Result.Unit(s),
    //             state.GetShrinkers<T>));
    // }
}
