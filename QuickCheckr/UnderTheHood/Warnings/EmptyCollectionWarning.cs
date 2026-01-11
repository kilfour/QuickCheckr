namespace QuickCheckr.UnderTheHood.Warnings;

public record EmptyCollectionWarning(string Key) : Warning
{
    public override string Message => $"Empty collection deemed irrelevant at input '{Key}'.";
}
