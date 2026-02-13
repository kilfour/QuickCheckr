using QuickCheckr.UnderTheHood.Docketing;

namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Custom;

public class CustomKeepShrinkClassifier : CustomClassifier<IShrinkOptionKeep>
{
    protected override InputShrinkingClassification GetClassification(IShrinkOptionKeep option, CustomShrinkingContext customContext)
    {
        var info = customContext.Info;
        var keep = InputShrinkingClue.Keep<CustomShrinkClassifier>(
            info.ExecutionId, info.Path, info.Value, customContext.Context.State.InputPrinter.IntroduceThis(info.Value));
        customContext.Context.State.Docket.Record(new InputShrinkingClueFound(keep));
        return new InputRelevant(customContext.Context.State.InputPrinter.IntroduceThis(info.Value));
    }
}
