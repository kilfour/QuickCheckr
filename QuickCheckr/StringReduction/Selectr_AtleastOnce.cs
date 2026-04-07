using QuickCheckr.StringReduction.Nuts;

namespace QuickCheckr.StringReduction;


public static partial class Selectr
{
    public static SelectrOf<SelectrSlice> AtleastOnce(params char[] options) =>
        AtleastOnce(a => options.Contains(a));

    public static SelectrOf<SelectrSlice> AtleastOnce(Func<char, bool> predicate) =>
        state =>
        {
            var length = 0;
            while (state.InBounds(length) && predicate(state.Text[state.Index + length]))
                length++;
            state.RunStopped = length == 0;
            var result = length > 0
                ? SelectrResult.Some<SelectrSlice>(state, SelectrSlice.From(state.Index, length, state.Text))
                : SelectrResult.Default<SelectrSlice>(state);
            state.Index += length;
            return result;
        };
}
