using QuickCheckr.StringReduction.Nuts;

namespace QuickCheckr.StringReduction;

public static partial class StringrExtensions
{
    /// <summary>
    /// Removes the selected range from each candidate produced by the script.
    /// Use to simplify an input by deleting matched text.
    /// </summary>
    public static SelectrOf<ITextSelection> Remove(this SelectrOf<ITextSelection> selectr) =>
        state =>
        {
            var result = selectr(state);
            if (!result.Succeeded)
                return SelectrResult.Default<ITextSelection>(state);

            var selection = result.Value;
            state.Remove(selection.AsSlice());
            return SelectrResult.Some(state, selection);
        };
}
