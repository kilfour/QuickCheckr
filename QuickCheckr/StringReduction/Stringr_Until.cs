using QuickCheckr.StringReduction.Nuts;

namespace QuickCheckr.StringReduction;


public static partial class Stringr
{
    public static SelectrOf<ITextSelection> Until(params char[] options) =>
        Until(a => options.Contains(a));

    public static SelectrOf<ITextSelection> Until(Func<char, bool> predicate) =>
        state =>
        {
            var length = 0;
            while (state.InBounds(length) && !predicate(state.Text[state.Index + length]))
                length++;
            var result = length > 0
                ? SelectrResult.Some(state, StringSlice.From(state.Index, length, state.Text))
                : SelectrResult.Default<ITextSelection>(state);
            state.Index += length;
            return result;
        };
}
