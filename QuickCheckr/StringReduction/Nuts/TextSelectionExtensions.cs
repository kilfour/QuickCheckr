namespace QuickCheckr.StringReduction.Nuts;

/// <summary>
/// Provides low-level conversions used by Stringr editing operations.
/// </summary>
public static class TextSelectionExtensions
{
    /// <summary>
    /// Returns the concrete slice represented by a text selection.
    /// Throws when a custom selection does not provide Stringr slice coordinates.
    /// </summary>
    public static StringSlice AsSlice(this ITextSelection selection) =>
        selection is StringSlice slice ? slice : throw new InvalidCastException();

    /// <summary>
    /// Returns the editable view of a text selection.
    /// Throws when the selection does not support deferred replacement.
    /// </summary>
    public static IEditable AsEditable(this ITextSelection selection) =>
        selection is IEditable editable ? editable : throw new InvalidCastException();
}
