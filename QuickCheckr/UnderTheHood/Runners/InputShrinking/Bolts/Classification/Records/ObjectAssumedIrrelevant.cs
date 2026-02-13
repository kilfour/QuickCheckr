namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Records;

public record ObjectAssumedIrrelevant(Dictionary<string, InputShrinkingClassification> Members)
    : InputShrinkingClassification;
