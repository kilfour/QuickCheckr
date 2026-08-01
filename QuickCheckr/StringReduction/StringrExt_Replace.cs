using QuickCheckr.StringReduction.Nuts;

namespace QuickCheckr.StringReduction;

public static partial class StringrExtensions
{
    /// <summary>
    /// Replaces the selected range using its selected text.
    /// Use to normalize or simplify a match as soon as it is selected.
    /// </summary>
    public static SelectrOf<ITextSelection> Replace(
        this SelectrOf<ITextSelection> selectr,
        Func<string, string> replacementFactory) =>
        state =>
        {
            var result = selectr(state);
            if (!result.Succeeded)
                return SelectrResult.Default<ITextSelection>(state);

            var selection = result.Value;
            var replacement = replacementFactory(selection.Text);
            state.Replace(selection.AsSlice(), replacement);
            return SelectrResult.Some(state, selection);
        };
}
