using QuickCheckr.UnderTheHood;

namespace QuickCheckr;

public static partial class Trackr
{
    public static CheckrOf<T> Capture<T>(Func<T> func) =>
        state => CheckrResult.Some(state, func());
    public static CheckrOf<T> Capture<T>(Task<T> func) =>
        state => CheckrResult.Some(state, func.GetAwaiter().GetResult());
}