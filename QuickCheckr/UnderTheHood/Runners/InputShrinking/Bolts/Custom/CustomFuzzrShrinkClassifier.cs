using QuickCheckr.UnderTheHood.Dispatching;

namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Custom;

public class CustomFuzzrShrinkClassifier : Classifier
{
    public override HandlerResult<InputShrinkingClassification> TryHandle(ShrinkingInfo info, ShrinkingContext context)
    {
        throw new NotImplementedException();
    }

}
