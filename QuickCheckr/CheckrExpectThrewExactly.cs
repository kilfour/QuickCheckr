using QuickCheckr.UnderTheHood;

namespace QuickCheckr;

public static partial class Checkr
{
    /// <summary>
    /// Checks the exact exception type and, when supplied, its exact message using ordinal equality.
    /// The message factory is evaluated once when the expectation runs, never when replay skips it.
    /// </summary>
    public static CheckrOf<Case> ExpectThrewExactly<TException>(
        string key, DelayedResult result, Func<string>? message = null)
        where TException : Exception
        => state =>
        {
            string? expectedMessage = null;
            return Evaluate.Expect(
                key,
                () =>
                {
                    if (message is not null) expectedMessage = message();
                    return result.ThrewExactly<TException>()
                        && (message is null || string.Equals(result.Exception!.Message, expectedMessage, StringComparison.Ordinal));
                },
                true,
                () =>
                {
                    var details = result.DescribeException<TException>();
                    if (message is not null) details.Add($"Expected message: {expectedMessage}");
                    return details;
                },
                () => new ExceptionFailureIdentity(result.Exception))(state);
        };
}
