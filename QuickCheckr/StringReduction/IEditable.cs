namespace QuickCheckr.StringReduction;

/// <summary>
/// Represents a text selection that can be edited by a later step in a Stringr query.
/// Use to defer a replacement until other selections have supplied the replacement value.
/// </summary>
public interface IEditable
{
    /// <summary>
    /// Creates a selector that replaces this selection using its original text.
    /// Use inside a LINQ query when the replacement depends on values selected by later steps.
    /// </summary>
    SelectrOf<IEditable> Replace(Func<string, string> replacementFactory);

    /// <summary>
    /// The text captured when this selection was created.
    /// </summary>
    string Text { get; }
}
