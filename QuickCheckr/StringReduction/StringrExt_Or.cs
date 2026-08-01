using QuickCheckr.StringReduction.Nuts;

namespace QuickCheckr.StringReduction;


public static partial class StringrExtensions
{
    public static SelectrOf<ITextSelection> Or(this SelectrOf<ITextSelection> left, SelectrOf<ITextSelection> right) =>
        state =>
        {
            var checkpoint = state.Capture();
            var leftResult = left(state);
            if (leftResult.Succeeded)
                return leftResult;

            state.Restore(checkpoint);
            var rightResult = right(state);
            if (rightResult.Succeeded)
                return rightResult;

            state.Restore(checkpoint);
            return SelectrResult.Default<ITextSelection>(state);
        };
}
