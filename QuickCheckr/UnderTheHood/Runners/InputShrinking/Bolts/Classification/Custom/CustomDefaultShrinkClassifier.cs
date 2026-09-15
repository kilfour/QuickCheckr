using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Defaults;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Records;

namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Custom;

internal sealed class CustomDefaultShrinkClassifier(DefaultClassifierDispatcher defaults) : CustomClassifier<IClassificationOptionDefault>
{
    protected override async ValueTask<InputClassification> GetClassificationAsync(IClassificationOptionDefault option, CustomShrinkingContext customContext)
        => await defaults.DispatchAsync(customContext.Info, customContext.Context).ConfigureAwait(false);
}
