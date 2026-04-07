using QuickCheckr.StringReduction.Nuts;

namespace QuickCheckr.StringReduction;


public static partial class SelectrExt
{
    public static SelectrOf<SelectrSlice> Or(this SelectrOf<SelectrSlice> left, SelectrOf<SelectrSlice> right) =>
        state =>
        {
            if (state.OutOfBounds()) return SelectrResult.Default<SelectrSlice>(state);
            var index = state.Index;
            var leftResult = left(state);
            if (leftResult.Value != null) return leftResult;
            state.Index = index;
            state.RunStopped = false;
            var rightResult = right(state);
            if (rightResult.Value != null) return rightResult;
            state.RunStopped = true;
            return SelectrResult.Default<SelectrSlice>(state);
        };
}
