using QuickCheckr;

namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Records;

public record ObjectRelevant(Dictionary<string, InputClassification> Members, string Report)
    : InputRelevant(Report)
{
    public override Maybe GetRedux()
    {
        if (Redux.HasValue) return Redux;
        if (!Members.Any(a => a.Value.GetRedux().HasValue)) return Maybe.Nothing();
        var values = Members.Where(a => a.Value is InputRelevant)
                .Select(kvp => GetReduxString(kvp.Key, (kvp.Value as InputRelevant)!));
        return Maybe.Just($"{{ {string.Join(", ", values)} }}");
    }

    private static string GetReduxString(string key, InputRelevant classification)
    {
        var reduxValue = classification.GetRedux();
        var value = reduxValue.HasValue ? reduxValue.Value : classification.Report;
        return $"{key}: {value}";
    }
}
