namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts;

public abstract record InputShrinkingClassification;

public record Unclassified() : InputShrinkingClassification;

public record InputAssumedIrrelevant() : InputShrinkingClassification;

public record ElementAssumedIrrelevant() : InputShrinkingClassification;

public record InputRelevant(string Report) : InputShrinkingClassification;

public record CollectionRelevant(IEnumerable<InputShrinkingClassification> Elements)
    : InputRelevant(GetReport(Elements))
{
    private static string GetReport(IEnumerable<InputShrinkingClassification> elements) =>
        !elements.Any() ? "[ ]" :
        $"[ {string.Join(", ",
            elements.Where(a => a is not ElementAssumedIrrelevant)
                .Select(a => a is InputRelevant relevant ? relevant.Report : "_"))} ]";
};

public record ObjectRelevant(Dictionary<string, InputShrinkingClassification> Members)
    : InputRelevant(GetReport(Members))
{
    private static string GetReport(Dictionary<string, InputShrinkingClassification> members) =>
        $"{{ {string.Join(", ",
            members.Where(a => a.Value is InputRelevant)
                .Select(kvp => $"{kvp.Key}: {(kvp.Value as InputRelevant)!.Report}"))} }}";
};

public static class IrrelevantCount
{
    public static int From(InputShrinkingClassification classification) =>
        classification switch
        {
            InputAssumedIrrelevant => 1,
            ElementAssumedIrrelevant => 1,
            CollectionRelevant c => c.Elements.Sum(From),
            ObjectRelevant o => o.Members.Values.Sum(From),
            _ => 0
        };
}
