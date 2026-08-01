namespace QuickCheckr.StringReduction;

/// <summary>
/// Provides fluent operations for composing, editing, and executing Stringr selectors.
/// </summary>
public static partial class StringrExtensions
{
    /// <summary>
    /// Converts a value-producing selector into a completed string-reduction script.
    /// Use after a fluent matcher or editor when the selected value is not otherwise needed.
    /// </summary>
    public static SelectrOf<Selection> Defined<T>(this SelectrOf<T> selection) =>
        from _ in selection select Selection.Defined;
}
