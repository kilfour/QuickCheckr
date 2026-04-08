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
            var slice = selection.AsSlice();
            var replacement = replacementFactory(slice.Text);
            state.Text = slice.Replace(state.Text, replacement);
            state.Index -= slice.Length;
            state.Index += replacement.Length;
            return SelectrResult.Some(state, selection);
        };
}
