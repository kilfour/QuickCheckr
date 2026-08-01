using QuickCheckr.StringReduction.Nuts;

namespace QuickCheckr.StringReduction;


public static partial class Stringr
{
    /// <summary>
    /// Selects one or more consecutive characters included in the supplied options.
    /// The selection fails when the current character is not included.
    /// </summary>
    public static SelectrOf<ITextSelection> AtleastOnce(params char[] options) =>
        AtleastOnce(a => options.Contains(a));

    /// <summary>
    /// Selects one or more consecutive characters satisfying the supplied predicate.
    /// Use for runs such as numbers, whitespace, or repeated operators.
    /// </summary>
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
