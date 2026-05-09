namespace QuickCheckr.UnderTheHood;

public record ExpectationFailure(string Key, List<string> Message)
{
    public static ExpectationFailure Empty => new("", []);
}