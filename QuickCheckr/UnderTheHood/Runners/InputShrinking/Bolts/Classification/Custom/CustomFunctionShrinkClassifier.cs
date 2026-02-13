namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Custom;

public class CustomFunctionShrinkClassifier : CustomClassifier<IShrinkOptionFunction>
{
    protected override InputShrinkingClassification GetClassification(IShrinkOptionFunction option, CustomShrinkingContext customContext)
        => new CustomShrinkClassifier(option.GetFunction()).Classify(customContext.Info, customContext.Context);
}
