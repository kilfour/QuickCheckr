using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Records;

namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Custom;

public class CustomNoneShrinkClassifier : CustomClassifier<IShrinkOptionNone>
{
    protected override InputShrinkingClassification GetClassification(IShrinkOptionNone option, CustomShrinkingContext customContext)
        => new Unclassified();
}
