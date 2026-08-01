using QuickCheckr.StringReduction.Nuts;

namespace QuickCheckr.StringReduction;


public static partial class Stringr
{
    public static SelectrOf<ITextSelection> AtleastOnce(params char[] options) =>
        AtleastOnce(a => options.Contains(a));

    public static SelectrOf<ITextSelection> AtleastOnce(Func<char, bool> predicate) =>
        state =>
        {
            var length = 0;
            while (state.InBounds(length) && predicate(state.Text[state.Index + length]))
                length++;

            if (length == 0)
                return SelectrResult.Default<ITextSelection>(state);

            var result = SelectrResult.Some<ITextSelection>(
                state,
                StringSlice.From(state.Index, length, state.Text));
            state.Advance(length);
            return result;
        };
}
