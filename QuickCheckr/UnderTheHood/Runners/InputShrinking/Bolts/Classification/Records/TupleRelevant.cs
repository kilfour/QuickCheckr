namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Records;

internal sealed record TupleRelevant(Dictionary<string, InputClassification> Members, string Report)
    : InputRelevant(Report, Maybe<Redux>.Nothing());
