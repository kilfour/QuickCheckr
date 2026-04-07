using QuickCheckr.StringReduction.Bolts;

namespace QuickCheckr.StringReduction.Nuts;


public static partial class Selectr
{
    public static SelectrOf<SelectrSlice> Optional(params char[] options) =>
        Optional(a => options.Contains(a));

    public static SelectrOf<SelectrSlice> Optional(Func<char, bool> predicate) =>
        state =>
        {
            var length = 0;
            while (state.InBounds(length) && predicate(state.Text[state.Index + length]))
                length++;
            var result = length > 0
                ? SelectrResult.Some<SelectrSlice>(state, SelectrSlice.From(state.Index, length, state.Text))
                : SelectrResult.Default<SelectrSlice>(state);
            state.Index += length;
            return result;
        };
}
