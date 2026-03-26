namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Records;

public record TupleRelevant(Dictionary<string, InputClassification> Members, string Report)
    : InputRelevant(Report, Maybe<Redux>.Nothing())
{
    // public override Maybe GetRedux()
    // {
    //     if (Redux.HasValue) return Redux;
    //     if (!Members.Any(a => a.Value.GetRedux().HasValue)) return Maybe.Nothing();
    //     var values = Members
    //             .Select(kvp => GetReduxString(kvp.Value));
    //     return Maybe.Just($"( {string.Join(", ", values)} )");
    // }

    // private static string GetReduxString(InputClassification classification)
    // {
    //     var reduxValue = classification.GetRedux();
    //     var value = reduxValue.HasValue ? reduxValue.Value : GetValue(classification);
    //     return $"{value}";
    // }

    // private static object GetValue(InputClassification classification)
    //     => classification switch
    //     {
    //         InputRelevant => (classification as InputRelevant)!.Report,
    //         _ => "_"
    //     };
}