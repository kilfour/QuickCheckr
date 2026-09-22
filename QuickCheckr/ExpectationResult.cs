using QuickCheckr.UnderTheHood;

namespace QuickCheckr;

public sealed record ExpectationResult
{
    internal bool IsPassed { get; }
    internal string[] Messages { get; }
    internal Func<ExceptionFailureIdentity>? FailureIdentity { get; }

    private ExpectationResult(
        bool isPassed,
        string[] messages,
        Func<ExceptionFailureIdentity>? failureIdentity = null)
    {
        IsPassed = isPassed;
        Messages = messages;
        FailureIdentity = failureIdentity;
    }

    internal static ExpectationResult Passed() => new(true, []);

    internal static ExpectationResult Failed(params string[] messages) =>
        new(false, messages);

    internal static ExpectationResult ExceptionFailed(
        Func<ExceptionFailureIdentity> failureIdentity,
        params string[] messages) =>
        new(false, messages, failureIdentity);
}
