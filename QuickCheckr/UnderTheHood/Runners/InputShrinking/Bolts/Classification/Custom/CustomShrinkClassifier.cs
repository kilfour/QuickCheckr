

using QuickCheckr.UnderTheHood.Docketing;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Records;

namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Custom;

public class CustomShrinkClassifier(Func<object, IEnumerable<object>> shrinker)
{
    public InputShrinkingClassification Classify(ShrinkingInfo info, ShrinkingContext context)
    {
        var verifier = new Verifier(context.State, context.Check);
        var values = shrinker(info.Value!);
        foreach (var candidate in values.Where(x => !Equals(x, info.Value)))
        {
            if (verifier.RunPassed(info.ExecutionId, info.Key, candidate!))
            {
                var clue = InputShrinkingClue.Keep<CustomShrinkClassifier>(
                    info.ExecutionId, info.Path, info.Value, context.State.InputPrinter.IntroduceThis(info.Value));
                context.State.Docket.Record(new InputShrinkingClueFound(clue));
                return new InputRelevant(context.State.InputPrinter.IntroduceThis(info.Value));
            }
        }
        var ignore = InputShrinkingClue.Ignore<CustomShrinkClassifier>(info.ExecutionId, info.Path, info.Value);
        context.State.Docket.Record(new InputShrinkingClueFound(ignore));
        return new InputAssumedIrrelevant();
    }
}