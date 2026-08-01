namespace QuickCheckr.StringReduction;

/// <summary>
/// Represents the successful completion of a Stringr selection script.
/// Use <see cref="Defined"/> as the final value of a LINQ query that produces reduction candidates.
/// </summary>
public readonly struct Selection
{
    /// <summary>
    /// The canonical completion marker for a Stringr selection script.
    /// </summary>
    public static readonly Selection Defined = default;
}
