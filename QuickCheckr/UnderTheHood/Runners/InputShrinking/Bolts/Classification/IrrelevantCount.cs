using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Records;

namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification;

public static class IrrelevantCount
{
    public static int From(InputShrinkingClassification classification) =>
        classification switch
        {
            InputAssumedIrrelevant => 1,
            ElementAssumedIrrelevant => 1,
            CollectionRelevant c => c.Elements.Sum(From),
            ObjectRelevant o => o.Members.Values.Sum(From),
            ObjectAssumedIrrelevant o => 1 + o.Members.Values.Sum(From),
            _ => 0
        };
}