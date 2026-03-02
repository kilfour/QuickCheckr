namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Records;

public record ObjectAssumedIrrelevant(Dictionary<string, InputClassification> Members)
    : InputClassification;
