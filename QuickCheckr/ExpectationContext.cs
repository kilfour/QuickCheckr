using QuickCheckr.UnderTheHood;
using QuickPulse.Show.Bolts;

namespace QuickCheckr;

public sealed class ExpectationContext(ManOfWealthAndTaste editor)
{
    public ExpectationResult Pass() =>
        ExpectationResult.Passed();

    public ExpectationResult Fail(params string[] messages) =>
        ExpectationResult.Failed(messages);

    public ExpectationResult Equal<T>(T? expected, T? actual)
    {
        if (EqualityComparer<T?>.Default.Equals(expected, actual))
            return ExpectationResult.Passed();
        return ExpectationResult.Failed(
            [
                $"Expected: {editor.IntroduceThis(expected)}",
                $"Actual: {editor.IntroduceThis(actual)}"
            ]);
    }

    public ExpectationResult ThrewExactly<TException>(
        DelayedResult result, string? message = null)
        where TException : Exception
    {
        if (result.ThrewExactly<TException>()
                && (message is null || string.Equals(result.Exception!.Message, message, StringComparison.Ordinal)))
            return ExpectationResult.Passed();

        var details = result.DescribeException<TException>();
        if (message is not null) details.Add($"Expected message: {message}");
        return ExpectationResult.ExceptionFailed(() => new ExceptionFailureIdentity(result.Exception), [.. details]);
    }

    public string Show<T>(T value) => editor.IntroduceThis(value);
}
