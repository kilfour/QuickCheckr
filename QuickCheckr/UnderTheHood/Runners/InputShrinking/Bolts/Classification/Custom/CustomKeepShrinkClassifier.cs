using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Records;

namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Custom;

internal sealed class CustomKeepShrinkClassifier : CustomClassifier<IClassificationOptionKeep>
{
    protected override ValueTask<InputClassification> GetClassificationAsync(
        IClassificationOptionKeep option,
        CustomShrinkingContext customContext)
        => new(InputRelevant.Create(customContext.PrintInput(customContext.Info.Value)));
}
