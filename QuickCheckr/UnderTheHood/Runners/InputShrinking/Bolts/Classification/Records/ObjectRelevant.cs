using QuickCheckr;

namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Records;

public record ObjectRelevant(Dictionary<string, InputShrinkingClassification> Members)
    : InputRelevant(GetReport(Members))
{
    private static string GetReport(Dictionary<string, InputShrinkingClassification> members) =>
        $"{{ {string.Join(", ",
            members.Where(a => a.Value is InputRelevant)
                .Select(kvp => $"{kvp.Key}: {(kvp.Value as InputRelevant)!.Report}"))} }}";
};
