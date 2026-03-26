using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Records;

namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification;

public static class IrrelevantCount
{
    public static int From(InputClassification classification) =>
        ((classification is InputRelevant inputRelevant)
            ? inputRelevant.Redux.HasValue ? 1 : 0
            : 0)
        + classification switch
        {
            InputAssumedIrrelevant => 1,
            ElementAssumedIrrelevant => 1,
            CollectionRelevant c => c.Elements.Sum(From),
            ObjectRelevant c => c.Members.Values.Sum(From),
            ObjectAssumedIrrelevant c => 1 + c.Members.Values.Sum(From),
            _ => 0
        };
}