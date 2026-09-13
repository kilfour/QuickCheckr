namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Records;

internal record InputRelevant(string Report, Maybe<Redux> Redux)
    : InputClassification
{
    public static InputRelevant Create(string report)
        => new(report, Maybe<Redux>.Nothing());
}
