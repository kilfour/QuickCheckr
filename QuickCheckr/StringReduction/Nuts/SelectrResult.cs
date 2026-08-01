namespace QuickCheckr.StringReduction.Nuts;

/// <summary>
/// Reports the value and outcome of a Stringr selection step.
/// Successful results may contain an empty text selection; inspect <see cref="Succeeded"/> rather than testing the value for null.
/// </summary>
public record SelectrResult<TValue>(SelectrState State, TValue Value, bool Succeeded);

/// <summary>
/// Creates successful and failed results for custom Stringr selectors.
/// </summary>
public static class SelectrResult
{
    /// <summary>
    /// Creates a successful selection result containing the supplied value.
    /// </summary>
    public static SelectrResult<TValue> Some<TValue>(SelectrState state, TValue value) =>
        new(state, value, true);

    /// <summary>
    /// Creates a failed selection result containing the default value for its type.
    /// </summary>
    public static SelectrResult<TValue> Default<TValue>(SelectrState state) =>
        new(state, default!, false);

    /// <summary>
    /// Creates a successful result containing the canonical <see cref="Selection.Defined"/> marker.
    /// </summary>
    public static SelectrResult<Selection> SelectionOnly(SelectrState state) =>
        Some(state, Selection.Defined);
}
