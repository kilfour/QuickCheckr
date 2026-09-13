namespace QuickCheckr.UnderTheHood;

internal sealed record VerificationFailure(string Key, List<string> Messages)
{
    public static VerificationFailure Empty => new("", []);
}
