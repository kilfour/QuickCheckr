namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Records;

public record TupleRelevant(Dictionary<string, InputClassification> Members, string Report)
    : InputRelevant(Report, Maybe<Redux>.Nothing());