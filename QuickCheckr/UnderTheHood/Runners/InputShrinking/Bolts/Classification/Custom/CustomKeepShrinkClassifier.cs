using QuickCheckr.UnderTheHood.Docketing;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Records;

namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Custom;

public class CustomKeepShrinkClassifier : CustomClassifier<IClassificationOptionKeep>
{
    protected override InputClassification GetClassification(
            IClassificationOptionKeep option,
            CustomShrinkingContext customContext)
    {
        var info = customContext.Info;
        var keep = InputShrinkingClue.Keep<CustomClassifierRunner>(
            info.ExecutionId, info.Path, info.Value, customContext.PrintInput(info.Value));
        customContext.Context.State.Docket.Record(new InputShrinkingClueFound(keep));
        return new InputRelevant(customContext.PrintInput(info.Value));
    }
}
