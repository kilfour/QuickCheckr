namespace QuickCheckr.UnderTheHood;

public record VerificationFailure(string Key, List<string> Messages)
{
    public static VerificationFailure Empty => new("", []);
}
