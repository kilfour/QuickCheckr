namespace QuickCheckr.StringReduction;

/// <summary>
/// Represents a range of text selected by a Stringr matcher.
/// Use its text in later query steps or transform the range with the Stringr editing extensions.
/// </summary>
public interface ITextSelection
{
    /// <summary>
    /// The text contained in the selected range.
    /// </summary>
    string Text { get; }
}
