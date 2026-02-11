

using QuickCheckr.UnderTheHood.Docketing;

namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Custom;

public class CustomShrinkStrategy(Func<object, IEnumerable<object>> shrinker)
{
    public InputShrinkingClassification Classify(CheckrState state, CheckrOf<Case> check, ShrinkingInfo info)
    {
        var verifier = new Verifier(state, check);
        var values = shrinker(info.Value!);
        foreach (var candidate in values.Where(x => !Equals(x, info.Value)))
        {
            if (verifier.RunPassed(info.ExecutionId, info.Key, candidate!))
            {
                var clue = InputShrinkingClue.Keep<CustomShrinkStrategy>(
                    info.ExecutionId, info.Path, info.Value, state.InputPrinter.IntroduceThis(info.Value));
                state.Docket.Record(new InputShrinkingClueFound(clue));
                return new InputRelevant(state.InputPrinter.IntroduceThis(info.Value));
            }
        }
        var ignore = InputShrinkingClue.Ignore<CustomShrinkStrategy>(info.ExecutionId, info.Path, info.Value);
        state.Docket.Record(new InputShrinkingClueFound(ignore));
        return new InputAssumedIrrelevant();
    }
}