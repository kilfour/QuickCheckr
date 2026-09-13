using QuickCheckr.UnderTheHood.Warnings;
using QuickCheckr.Protocol;

namespace QuickCheckr.UnderTheHood.Proceedings.ClerksOffice;

internal sealed record CompositeEmptyCollectionWarning(IEnumerable<string> Entries) : Warning(WarningLevel.Debug)
{
    public override string Message =>
@$"Empty {Noun(Entries.Count())} deemed irrelevant at:
{string.Join(Environment.NewLine, Entries)}
       {EmptyCollectionWarning.Suggestion}";

    private static string Noun(int count) => count == 1 ? "collection" : "collections";
}
