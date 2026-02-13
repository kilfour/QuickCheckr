using QuickCheckr.UnderTheHood.Dispatching;
using QuickCheckr.UnderTheHood.Docketing;

namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Custom;

public class CustomFallbackClassifier : FirstMatchCollectionFallback<CustomShrinkingContext, InputShrinkingClassification>
{
    public override InputShrinkingClassification Handle(CustomShrinkingContext context)
    {
        var info = context.Info;
        var clue = InputShrinkingClue.Ignore<CustomShrinkClassifier>(info.ExecutionId, info.Path, info.Value);
        context.Context.State.Docket.Record(new InputShrinkingClueFound(clue));
        return new InputAssumedIrrelevant();
    }
}
