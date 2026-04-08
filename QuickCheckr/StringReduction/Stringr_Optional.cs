using QuickCheckr.StringReduction.Nuts;

namespace QuickCheckr.StringReduction;


public static partial class Stringr
{
    public static SelectrOf<ITextSelection> Optional(params char[] options) =>
        Optional(a => options.Contains(a));

    public static SelectrOf<ITextSelection> Optional(Func<char, bool> predicate) =>
        state =>
        {
            var length = 0;
            while (state.InBounds(length) && predicate(state.Text[state.Index + length]))
                length++;
            var result = length > 0
                ? SelectrResult.Some(state, StringSlice.From(state.Index, length, state.Text))
                : SelectrResult.Default<ITextSelection>(state);
            state.Advance(length);
            return result;
        };
}
