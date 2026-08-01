using QuickCheckr.StringReduction.Nuts;

namespace QuickCheckr.StringReduction;


/// <summary>
/// Entry point for building composable string-reduction selectors.
/// Use the static members of this class to match text before removing or replacing it.
/// </summary>
public static partial class Stringr
{
    /// <summary>
    /// Selects one character when it is included in the supplied options.
    /// With no options, selects any character at the current position.
    /// </summary>
    public static SelectrOf<ITextSelection> Once(params char[] options)
    {
        if (options.Length == 0)
            return Once(_ => true);
        return Once(a => options.Contains(a));
    }

    /// <summary>
    /// Selects one character when it satisfies the supplied predicate.
    /// Use for character classes such as digits or whitespace.
    /// </summary>
    public static SelectrOf<ITextSelection> Once(Func<char, bool> predicate) =>
        state =>
        {
            if (state.OutOfBounds())
                return SelectrResult.Default<ITextSelection>(state);

            if (predicate(state.Text[state.Index]))
            {
                var result = StringSlice.From(state.Index, 1, state.Text);
                state.Advance();
                return SelectrResult.Some(state, result);
            }
            return SelectrResult.Default<ITextSelection>(state);
        };
}
