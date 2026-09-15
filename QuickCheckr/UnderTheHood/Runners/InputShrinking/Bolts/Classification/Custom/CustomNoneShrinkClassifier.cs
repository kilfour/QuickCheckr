using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Records;

namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Custom;

internal sealed class CustomNoneShrinkClassifier : CustomClassifier<IClassificationOptionNone>
{
    protected override ValueTask<InputClassification> GetClassificationAsync(IClassificationOptionNone option, CustomShrinkingContext customContext)
        => new(new Unclassified());
}
