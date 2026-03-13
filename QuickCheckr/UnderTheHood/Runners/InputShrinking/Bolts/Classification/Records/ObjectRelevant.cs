using QuickCheckr;

namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Records;

public record ObjectRelevant(Dictionary<string, InputClassification> Members)
    : InputRelevant(GetReport(Members))
{
    private static string GetReport(Dictionary<string, InputClassification> members) =>
        $"{{ {string.Join(", ",
            members.Where(a => a.Value is InputRelevant)
                .Select(kvp => $"{kvp.Key}: {(kvp.Value as InputRelevant)!.Report}"))} }}";

    public override Maybe GetRedux()
    {
        if (Redux.HasValue) return Redux;
        if (!Members.Any(a => a.Value.GetRedux().HasValue)) return Maybe.Nothing();
        var values = Members.Where(a => a.Value is InputRelevant)
                .Select(kvp => GetReduxString(kvp.Key, (kvp.Value as InputRelevant)!));
        return Maybe.Just($"{{ {string.Join(", ", values)} }}");
    }

    private string GetReduxString(string key, InputRelevant classification)
    {
        var reduxValue = classification.GetRedux();
        var value = reduxValue.HasValue ? reduxValue.Value : classification.Report;
        return $"{key}: {value}";
    }
}