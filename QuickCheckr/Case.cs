namespace QuickCheckr;

/// <summary>
/// Represents the successful completion of a Checkr script.  
/// Use <see cref="Closed"/> as the final value in a LINQ chain to signal that the case has no further steps.
/// </summary>
public readonly struct Case
{
    /// <summary>
    /// The canonical end-marker for a Checkr case.  
    /// Use when your script completes successfully and does not need to return any additional data.
    /// </summary>
    public static readonly Case Closed = default;
}

