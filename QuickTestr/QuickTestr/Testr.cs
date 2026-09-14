using QuickTestr.Bolts.Builders;

namespace QuickTestr;

/// <summary>
/// Creates fluent QuickTestr definitions.
/// Use as the entry point for building property-based or oracle-based tests.
/// </summary>
public static class Testr
{
    /// <summary>
    /// Starts a new Testr with the given name.
    /// Use to define a readable label for the property or comparison being tested.
    /// </summary>
    public static NamedTestr Named(string name) => new(name);
}
