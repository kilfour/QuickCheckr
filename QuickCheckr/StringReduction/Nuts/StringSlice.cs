namespace QuickCheckr.StringReduction.Nuts;



/// <summary>
/// Represents a concrete range selected from a string.
/// The captured text remains available when the range is edited later in a query.
/// </summary>
public sealed record StringSlice(int Start, int Length, string Text) : ITextSelection, IEditable
{
    /// <summary>
    /// Creates a slice from the supplied input range and captures its text.
    /// </summary>
    public static ITextSelection From(int start, int length, string input)
        => new StringSlice(start, length, input.Substring(start, length));

    /// <summary>
    /// Creates a selector that applies a deferred replacement to this slice.
    /// </summary>
    public SelectrOf<IEditable> Replace(Func<string, string> replacementFactory)
    {
        return state =>
        {
            var replacement = replacementFactory(Text);
            state.LateReplace(this, replacement);
            return SelectrResult.Some<IEditable>(state, this);
        };
    }
}
