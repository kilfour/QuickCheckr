using QuickCheckr.UnderTheHood;
using QuickFuzzr;

namespace QuickCheckr;

public static partial class Trackr
{
    public static CheckrOf<Case> ClearPool<T>() =>
        state =>
        {
            state.Memory.ClearPool<T>();
            return CheckrResult.Default<Case>(state);
        };
}
