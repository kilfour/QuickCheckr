using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Records;

namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Custom;

public class CustomKeepShrinkClassifier : CustomClassifier<IClassificationOptionKeep>
{
    protected override InputClassification GetClassification(
        IClassificationOptionKeep option,
        CustomShrinkingContext customContext)
        => InputRelevant.Create(customContext.PrintInput(customContext.Info.Value));
}
