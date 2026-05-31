namespace QuickCheckr;

/// <summary>
/// Represents how many times the Checkr should be run.  
/// Use to expand exploration across multiple independent input sets.
/// </summary>
public record RunCount(int NumberOfRuns);
