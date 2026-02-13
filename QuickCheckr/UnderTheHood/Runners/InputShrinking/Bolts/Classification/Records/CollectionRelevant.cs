using QuickCheckr;

namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Records;

public record CollectionRelevant(IEnumerable<InputShrinkingClassification> Elements)
    : InputRelevant(GetReport(Elements))
{
    private static string GetReport(IEnumerable<InputShrinkingClassification> elements) =>
        !elements.Any() ? "[ ]" :
        $"[ {string.Join(", ",
            elements.Where(a => a is not ElementAssumedIrrelevant)
                .Select(a => a is InputRelevant relevant ? relevant.Report : "_"))} ]";
};
