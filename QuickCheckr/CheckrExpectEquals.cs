namespace QuickCheckr;

public static partial class Checkr
{
    /// <summary>
    /// Adds a named expectation that compares two values for equality.
    /// When the values differ, the expected and actual values are included in the failure report
    /// using the configured trace formatter.
    /// </summary>
    public static CheckrOf<Case> ExpectEquals<T>(string key, T? expected, T? actual) =>
        Expect(
            key,
            () => EqualityComparer<T?>.Default.Equals(expected, actual),
            editor =>
            [
                $"Expected: {editor.IntroduceThis(expected)}",
                $"Actual: {editor.IntroduceThis(actual)}"
            ]);
}
