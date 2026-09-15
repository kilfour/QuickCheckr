using QuickCheckr.UnderTheHood.Dispatching;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Records;

namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Custom;

internal abstract class CustomClassifier<T>
    : FirstMatchHandler<IClassificationOption, CustomShrinkingContext, InputClassification>
{
    public override async ValueTask<HandlerResult<InputClassification>> TryHandleAsync(
        IClassificationOption input, CustomShrinkingContext customContext)
        => input is T option
            ? ReturnIfRelevant(await GetClassificationAsync(option, customContext).ConfigureAwait(false))
            : Unhandled;

    private HandlerResult<InputClassification> ReturnIfRelevant(InputClassification classification)
        => classification is InputAssumedIrrelevant
            ? Unhandled
            : HandlerResult.Handled(classification);

    protected abstract ValueTask<InputClassification> GetClassificationAsync(T input, CustomShrinkingContext customContext);
}
