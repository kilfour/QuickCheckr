using QuickCheckr;

namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Records;

public record CollectionRelevant(IEnumerable<InputClassification> Elements)
    : InputRelevant(GetReport(Elements), Maybe<Redux>.Nothing())
{
    private static string GetReport(IEnumerable<InputClassification> elements) =>
        !elements.Any() ? "[ ]" :
        $"[ {string.Join(", ",
            elements.Where(a => a is not ElementAssumedIrrelevant)
                .Select(a => a is InputRelevant relevant ? relevant.Report : "_"))} ]";
};
