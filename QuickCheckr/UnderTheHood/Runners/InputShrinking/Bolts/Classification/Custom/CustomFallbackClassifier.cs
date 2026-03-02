using QuickCheckr.UnderTheHood.Dispatching;
using QuickCheckr.UnderTheHood.Docketing;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Records;

namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Custom;

public class CustomFallbackClassifier : FirstMatchCollectionFallback<CustomShrinkingContext, InputClassification>
{
    public override InputClassification Handle(CustomShrinkingContext context)
    {
        var info = context.Info;
        var clue = InputShrinkingClue.Ignore<CustomClassifierRunner>(info.ExecutionId, info.Path, info.Value);
        context.Context.State.Docket.Record(new InputShrinkingClueFound(clue));
        return new InputAssumedIrrelevant();
    }
}
