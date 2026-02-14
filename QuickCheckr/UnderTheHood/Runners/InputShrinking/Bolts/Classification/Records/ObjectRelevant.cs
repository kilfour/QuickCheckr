using QuickCheckr;

namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Records;

public record ObjectRelevant(Dictionary<string, InputShrinkingClassification> Members)
    : InputRelevant(GetReport(Members))
{
    private static string GetReport(Dictionary<string, InputShrinkingClassification> members) =>
        $"{{ {string.Join(", ",
            members.Where(a => a.Value is InputRelevant)
                .Select(kvp => $"{kvp.Key}: {(kvp.Value as InputRelevant)!.Report}"))} }}";
    public override Maybe GetRedux()
    {
        if (Redux.HasValue) return Redux;
        var childRedux = Members.Where(a => a.Value.GetRedux().HasValue).ToList();
        if (childRedux.Count == 0) return Maybe.Nothing();
        return Maybe.Just($"{{ {string.Join(", ", childRedux.Select(kvp => $"{kvp.Key}: {kvp.Value.Redux.Value}"))} }}");
    }
}