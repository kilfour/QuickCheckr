namespace QuickCheckr.UnderTheHood;

public record ExpectationFailure(
    string Key,
    List<string> Message,
    ExceptionFailureIdentity? Identity = null)
{
    public static ExpectationFailure Empty => new("", []);

    public bool IsEquivalentTo(ExpectationFailure other)
    {
        if (Key != other.Key)
            return false;
        if (Identity is null || other.Identity is null)
            return Identity is null && other.Identity is null;
        return Identity.IsEquivalentTo(other.Identity);
    }
}

public record ExceptionFailureIdentity(Exception? Exception)
{
    public bool IsEquivalentTo(ExceptionFailureIdentity other)
        => ExceptionIs.Equivalent(Exception, other.Exception)
            && Exception?.TargetSite == other.Exception?.TargetSite;
}
