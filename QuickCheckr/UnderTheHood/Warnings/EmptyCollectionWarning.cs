using QuickCheckr.Protocol;

namespace QuickCheckr.UnderTheHood.Warnings;

public sealed record EmptyCollectionWarning(string Key, string Path) : Warning(WarningLevel.Important)
{
    public const string Suggestion =
        "If emptiness may hide the failure's cause, provide a non-empty witness with Shrink.Values(...) or opt into sampling with Shrink.Fuzzr(...).";

    public override string Message =>
        $"Empty collection deemed irrelevant at input '{Key}'. {Suggestion}";
}
