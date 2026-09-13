using QuickCheckr.UnderTheHood.Dispatching;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Records;
using QuickCheckr.UnderTheHood.Warnings;

namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Defaults;

internal sealed class FallbackClassifier : FirstMatchFallback<ShrinkingInfo, ShrinkingContext, InputClassification>
{
    public override InputClassification Handle(ShrinkingInfo info, ShrinkingContext context)
    {
        context.Warn(info.Path, () => new CantShrinkWarning(info.Type, info.Path));
        return InputRelevant.Create(context.PrintInput(info.Value));
    }
}
