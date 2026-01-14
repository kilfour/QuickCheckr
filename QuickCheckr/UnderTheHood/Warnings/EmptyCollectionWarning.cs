using QuickPulse.Show.Bolts;

namespace QuickCheckr.UnderTheHood.Warnings;

public record EmptyCollectionWarning(string Key, string Path) : Warning
{
    public override string Message => $"Empty collection deemed irrelevant at input '{Key}'.";
}

public record CompositeEmptyCollectionWarning(IEnumerable<string> Entries) : Warning
{
    public override string Message =>
@$"Empty {Noun(Entries.Count())} deemed irrelevant at:
{string.Join(Environment.NewLine, Entries)}";

    private static string Noun(int count) => count == 1 ? "collection" : "collections";
}
