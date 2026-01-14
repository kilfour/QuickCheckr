using QuickPulse.Show.Bolts;

namespace QuickCheckr.UnderTheHood.Warnings;

public record EmptyCollectionWarning(string Key, string Path) : Warning
{
    public override string Message => $"Empty collection deemed irrelevant at input '{Key}'.";
}
