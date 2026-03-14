using QuickCheckr;

namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Records;

public record CollectionRelevant(IEnumerable<InputClassification> Elements, string Witness = "")
    : InputRelevant(GetReport(Elements))
{
    private static string GetReport(IEnumerable<InputClassification> elements) =>
        !elements.Any() ? "[ ]" :
        $"[ {string.Join(", ",
            elements.Where(a => a is not ElementAssumedIrrelevant)
                .Select(a => a is InputRelevant relevant ? relevant.Report : "_"))} ]";

    public override Maybe GetRedux()
    {
        if (Redux.HasValue) return Redux;
        if (!Elements.Any(a => a.GetRedux().HasValue)) return Maybe.Nothing();
        var values = Elements.Where(a => a is InputRelevant)
                .Select(a => GetReduxString((a as InputRelevant)!));
        return Maybe.Just($"[ {string.Join(", ", values)} ]");
    }

    private string GetReduxString(InputRelevant classification)
    {
        var reduxValue = classification.GetRedux();
        return reduxValue.HasValue ? reduxValue.Value! : classification.Report;
    }
};
