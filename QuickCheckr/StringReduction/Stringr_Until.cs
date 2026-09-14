using QuickCheckr.StringReduction.Nuts;

namespace QuickCheckr.StringReduction;


public static partial class Stringr
{
    /// <summary>
    /// Selects consecutive characters up to, but not including, one of the supplied delimiters.
    /// Produces an empty selection when the current character is already a delimiter.
    /// </summary>
    public static SelectrOf<ITextSelection> Until(params char[] options) =>
        Until(a => options.Contains(a));

    /// <summary>
    /// Selects consecutive characters up to, but not including, a character satisfying the predicate.
    /// Use to capture the contents preceding a delimiter selected by a later step.
    /// </summary>
    public static SelectrOf<ITextSelection> Until(Func<char, bool> predicate) =>
        state =>
        {
            var length = 0;
            while (state.InBounds(length) && !predicate(state.Text[state.Index + length]))
                length++;

            var result = SelectrResult.Some(
                state,
                StringSlice.From(state.Index, length, state.Text));
            state.Advance(length);
            return result;
        };
}
