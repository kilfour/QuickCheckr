using QuickCheckr.Protocol;

namespace QuickCheckr.UnderTheHood.Warnings;

public record EmptyCollectionWarning(string Key, string Path) : Warning(WarningLevel.Verbose)
{
    public override string Message => $"Empty collection deemed irrelevant at input '{Key}'.";
}
