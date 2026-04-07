using QuickCheckr.StringReduction.Nuts;

namespace QuickCheckr.StringReduction;

public static partial class SelectrExt
{
    public static SelectrOf<string> Remove(this SelectrOf<SelectrSlice> selection) =>
        state =>
        {
            var slice = selection(state).Value as SelectrSlice;
            if (slice == null) return SelectrResult.Default<string>(state);
            var result = slice.GetText(state.Text);
            state.Text = slice.Remove(state.Text);
            state.Index -= slice.Length;
            return SelectrResult.Some(state, result);
        };
}
