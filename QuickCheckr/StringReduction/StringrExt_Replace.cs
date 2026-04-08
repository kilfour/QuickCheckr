using QuickCheckr.StringReduction.Nuts;

namespace QuickCheckr.StringReduction;

public static partial class StringrExtensions
{
    public static SelectrOf<ITextSelection> Replace(
        this SelectrOf<ITextSelection> selectr,
        Func<string, string> replacementFactory) =>
        state =>
        {
            var selection = selectr(state).Value;
            if (selection == null) return SelectrResult.Default<ITextSelection>(state);
            var replacement = replacementFactory(selection.Text);
            state.Replace(selection.AsSlice(), replacement);
            return SelectrResult.Some(state, selection);
        };
}
