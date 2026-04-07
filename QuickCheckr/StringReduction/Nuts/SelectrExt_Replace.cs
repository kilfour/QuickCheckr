using QuickCheckr.StringReduction.Bolts;

namespace QuickCheckr.StringReduction.Nuts;

public static partial class SelectrExt
{
    public static SelectrOf<string> Replace(this SelectrOf<ISelection> selection, string replacement) =>
        state =>
        {
            var slice = selection(state).Value as Slice;
            if (slice == null) return SelectrResult.Default<string>(state);
            var result = slice.GetText(state.Text);
            state.Text = slice.Replace(state.Text, replacement);
            // state.Index -= slice.Length;
            // state.Index += replacement.Length;
            return SelectrResult.Some(state, result);
        };
}
