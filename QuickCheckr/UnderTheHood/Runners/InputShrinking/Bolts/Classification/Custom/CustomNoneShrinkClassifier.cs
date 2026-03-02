using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Records;

namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Custom;

public class CustomNoneShrinkClassifier : CustomClassifier<IClassificationOptionNone>
{
    protected override InputClassification GetClassification(IClassificationOptionNone option, CustomShrinkingContext customContext)
        => new Unclassified();
}
