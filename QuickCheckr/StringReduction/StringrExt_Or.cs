using QuickCheckr.StringReduction.Nuts;

namespace QuickCheckr.StringReduction;


public static partial class StringrExtensions
{
    /// <summary>
    /// Selects with the left alternative, falling back to the right alternative when it does not match.
    /// Changes made by a failed alternative are rolled back before the next alternative is attempted.
    /// </summary>
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
