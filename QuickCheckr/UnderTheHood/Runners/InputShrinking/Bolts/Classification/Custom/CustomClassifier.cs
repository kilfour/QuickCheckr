using QuickCheckr.UnderTheHood.Dispatching;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Records;

namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Custom;

internal abstract class CustomClassifier<T>
    : FirstMatchHandler<IClassificationOption, CustomShrinkingContext, InputClassification>
{
    public override HandlerResult<InputClassification> TryHandle(IClassificationOption input, CustomShrinkingContext customContext)
        => ReturnIfRelevant(input, customContext);

    private HandlerResult<InputClassification> ReturnIfRelevant(IClassificationOption input, CustomShrinkingContext ctx)
    => input is T option
        ? ReturnIfRelevant(GetClassification(option, ctx))
        : Unhandled;

    private HandlerResult<InputClassification> ReturnIfRelevant(InputClassification classification)
        => classification is InputAssumedIrrelevant
            ? Unhandled
            : HandlerResult.Handled(classification);

    protected abstract InputClassification GetClassification(T input, CustomShrinkingContext customContext);
}
