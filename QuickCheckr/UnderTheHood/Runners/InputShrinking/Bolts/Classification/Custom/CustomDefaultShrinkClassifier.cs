using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Defaults;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Records;

namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Custom;

public class CustomDefaultShrinkClassifier(DefaultClassifierDispatcher defaults) : CustomClassifier<IShrinkOptionDefault>
{
    protected override InputShrinkingClassification GetClassification(IShrinkOptionDefault option, CustomShrinkingContext customContext)
        => defaults.Dispatch(customContext.Info, customContext.Context);
}
