namespace QuickCheckr;
public static partial class Checkr
{
    public static CheckrOf<Case> ExpectEquals<T>(string key, Func<T?> expected, Func<Task<T?>> actual)
        => ExpectEqualsAsync<T>(key, _ => Task.FromResult(expected()), _ => actual());

    public static CheckrOf<Case> ExpectEquals<T>(string key, Func<T?> expected, Func<CancellationToken, Task<T?>> actual)
        => ExpectEqualsAsync<T>(key, _ => Task.FromResult(expected()), actual);

    public static CheckrOf<Case> ExpectEquals<T>(string key, Func<Task<T?>> expected, Func<T?> actual)
        => ExpectEqualsAsync<T>(key, _ => expected(), _ => Task.FromResult(actual()));

    public static CheckrOf<Case> ExpectEquals<T>(string key, Func<Task<T?>> expected, Func<Task<T?>> actual)
        => ExpectEqualsAsync<T>(key, _ => expected(), _ => actual());

    public static CheckrOf<Case> ExpectEquals<T>(string key, Func<Task<T?>> expected, Func<CancellationToken, Task<T?>> actual)
        => ExpectEqualsAsync<T>(key, _ => expected(), actual);

    public static CheckrOf<Case> ExpectEquals<T>(string key, Func<CancellationToken, Task<T?>> expected, Func<T?> actual)
        => ExpectEqualsAsync<T>(key, expected, _ => Task.FromResult(actual()));

    public static CheckrOf<Case> ExpectEquals<T>(string key, Func<CancellationToken, Task<T?>> expected, Func<Task<T?>> actual)
        => ExpectEqualsAsync<T>(key, expected, _ => actual());

    public static CheckrOf<Case> ExpectEquals<T>(string key, Func<CancellationToken, Task<T?>> expected, Func<CancellationToken, Task<T?>> actual)
        => ExpectEqualsAsync<T>(key, expected, actual);

    private static CheckrOf<Case> ExpectEqualsAsync<T>(string key,
        Func<CancellationToken, Task<T?>> expected, Func<CancellationToken, Task<T?>> actual) =>
        state =>
        {
            T? expectedValue = default;
            T? actualValue = default;
            return Expect(key, async token =>
            {
                expectedValue = await expected(token).ConfigureAwait(false);
                token.ThrowIfCancellationRequested();
                actualValue = await actual(token).ConfigureAwait(false);
                token.ThrowIfCancellationRequested();
                return EqualityComparer<T?>.Default.Equals(expectedValue, actualValue);
            }, editor =>
            [
                $"Expected: {editor.IntroduceThis(expectedValue)}",
                $"Actual: {editor.IntroduceThis(actualValue)}"
            ])(state);
        };
}
