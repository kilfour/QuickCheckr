using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Records;

namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Custom;

internal sealed class CustomFunctionShrinkClassifier : CustomClassifier<IClassificationOptionFunction>
{
    protected override async ValueTask<InputClassification> GetClassificationAsync(IClassificationOptionFunction option, CustomShrinkingContext customContext)
        => await new CustomClassifierRunner(option.GetFunction()).ClassifyAsync(customContext.Info, customContext.Context).ConfigureAwait(false);
}
