using QuickCheckr.StringReduction.Nuts;

namespace QuickCheckr.StringReduction;


public static partial class Stringr
{
    /// <summary>
    /// Selects zero or more consecutive characters included in the supplied options.
    /// Produces an empty selection when the current character is not included.
    /// </summary>
    public static SelectrOf<ITextSelection> Optional(params char[] options) =>
        Optional(a => options.Contains(a));

    /// <summary>
    /// Selects zero or more consecutive characters satisfying the supplied predicate.
    /// Use for optional runs such as whitespace around another selection.
    /// </summary>
    public static SelectrOf<ITextSelection> Optional(Func<char, bool> predicate) =>
        state =>
        {
            var length = 0;
            while (state.InBounds(length) && predicate(state.Text[state.Index + length]))
                length++;

            var result = SelectrResult.Some<ITextSelection>(
                state,
                StringSlice.From(state.Index, length, state.Text));
            state.Advance(length);
            return result;
        };
}
