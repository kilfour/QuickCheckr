using QuickCheckr.UnderTheHood.Docketing;

namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Primitives;

public class EnumShrinkClassifier
{
    public InputShrinkingClassification Classify(CheckrState state, CheckrOf<Case> check, ShrinkingInfo info)
    {
        var verifier = new Verifier(state, check);
        var enumVals = GetEnumValues(info.Type);
        foreach (var candidate in enumVals.Where(x => !Equals(x, info.Value)))
        {
            if (verifier.RunPassed(info.ExecutionId, info.Key, candidate))
            {
                // Mark input as relevant, because we found a candidate that makes the run pass.
                var clue = InputShrinkingClue.Keep<EnumShrinkClassifier>(info.ExecutionId, info.Path, info.Value, state.InputPrinter.IntroduceThis(info.Value));
                state.Docket.Record(new InputShrinkingClueFound(clue));
                return new InputRelevant(state.InputPrinter.IntroduceThis(info.Value));
            }
        }
        var ignoreClue = InputShrinkingClue.Ignore<EnumShrinkClassifier>(info.ExecutionId, info.Path, info.Value);
        state.Docket.Record(new InputShrinkingClueFound(ignoreClue));
        return new InputAssumedIrrelevant();
    }

    private static IEnumerable<object> GetEnumValues(Type type)
    {
        if (!type.IsEnum) return Array.Empty<object>();
        return Enum.GetValues(type).Cast<object>();
    }
}
