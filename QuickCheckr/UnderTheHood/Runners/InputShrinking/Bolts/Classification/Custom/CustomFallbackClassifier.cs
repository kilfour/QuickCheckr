using QuickCheckr.UnderTheHood.Dispatching;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Records;

namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Custom;

public class CustomFallbackClassifier : FirstMatchCollectionFallback<CustomShrinkingContext, InputClassification>
{
    public override InputClassification Handle(CustomShrinkingContext context)
    {
        return new Unclassified();
    }
}
