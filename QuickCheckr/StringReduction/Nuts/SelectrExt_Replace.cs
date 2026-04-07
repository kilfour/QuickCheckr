using QuickCheckr.StringReduction.Bolts;

namespace QuickCheckr.StringReduction.Nuts;

public static partial class SelectrExt
{
    public static SelectrOf<string> Replace(
        this SelectrOf<SelectrSlice> selection,
        Func<string, string> replacementFactory) =>
        state =>
        {
            var slice = selection(state).Value;
            if (slice == null) return SelectrResult.Default<string>(state);
            var result = slice.GetText(state.Text);
            var replacement = replacementFactory(result);
            state.Text = slice.Replace(state.Text, replacement);
            state.Index -= slice.Length;
            state.Index += replacement.Length;
            return SelectrResult.Some(state, result);
        };
}
