namespace QuickTestr;

/// <summary>
/// Specifies how many independent searches are used when filling the vault.
/// Use to control how broadly QuickTestr looks for distinct failing cases.
/// </summary>
public record SearchCount(int NumberOfSearches);


/// <summary>
/// Adds fluent helpers for creating <see cref="SearchCount"/> values.
/// Use when you want a readable way to specify vault search counts from integers.
/// </summary>
public static class SearchCountExtensions
{
    /// <summary>
    /// Creates a <see cref="SearchCount"/> from the number of independent searches to run.
    /// Use when configuring vault filling with an integer literal or variable.
    /// </summary>
    public static SearchCount Searches(this int numberOfSearches) => new(numberOfSearches);
}
