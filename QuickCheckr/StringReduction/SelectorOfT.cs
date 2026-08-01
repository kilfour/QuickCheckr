using QuickCheckr.StringReduction.Nuts;

namespace QuickCheckr.StringReduction;

/// <summary>
/// Represents a composable Stringr selection step.
/// Each step reads or edits the current text state and reports whether it selected a value.
/// </summary>
public delegate SelectrResult<T> SelectrOf<T>(SelectrState state);
