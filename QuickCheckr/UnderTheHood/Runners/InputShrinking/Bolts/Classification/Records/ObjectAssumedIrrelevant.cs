namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Records;

internal sealed record ObjectAssumedIrrelevant(Dictionary<string, InputClassification> Members)
    : InputClassification;
