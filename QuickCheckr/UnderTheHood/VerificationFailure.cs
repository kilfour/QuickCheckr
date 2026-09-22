namespace QuickCheckr.UnderTheHood;

internal sealed record VerificationFailure(string Key, string[] Messages)
{
    public static VerificationFailure Empty => new("", []);
}
