using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Records;

namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Custom;

public class CustomFunctionShrinkClassifier : CustomClassifier<IClassificationOptionFunction>
{
    protected override InputClassification GetClassification(IClassificationOptionFunction option, CustomShrinkingContext customContext)
        => new CustomClassifierRunner(option.GetFunction()).Classify(customContext.Info, customContext.Context);
}
