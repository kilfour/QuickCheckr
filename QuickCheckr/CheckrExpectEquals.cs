namespace QuickCheckr;

public static partial class Checkr
{
    /// <summary>
    /// Compares values evaluated only when this expectation runs, after replay skip checks.
    /// Each factory is evaluated once per expectation execution; failures report those values.
    /// </summary>
    public static CheckrOf<Case> ExpectEquals<T>(string key, Func<T?> expected, Func<T?> actual) =>
        state =>
        {
            T? expectedValue = default;
            T? actualValue = default;
            return Expect(
                key,
                () =>
                {
                    expectedValue = expected();
                    actualValue = actual();
                    return EqualityComparer<T?>.Default.Equals(expectedValue, actualValue);
                },
                editor =>
                [
                    $"Expected: {editor.IntroduceThis(expectedValue)}",
                    $"Actual: {editor.IntroduceThis(actualValue)}"
                ])(state);
        };
}
