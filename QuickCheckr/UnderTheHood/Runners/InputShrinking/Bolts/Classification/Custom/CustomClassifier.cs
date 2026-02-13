using QuickCheckr.UnderTheHood.Dispatching;

namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Custom;

public abstract class CustomClassifier<T>
    : FirstMatchHandler<IShrinkOption, CustomShrinkingContext, InputShrinkingClassification>
{
    public override HandlerResult<InputShrinkingClassification> TryHandle(IShrinkOption input, CustomShrinkingContext customContext)
        => ReturnIfRelevant(input, customContext);

    private HandlerResult<InputShrinkingClassification> ReturnIfRelevant(IShrinkOption input, CustomShrinkingContext ctx)
    => input is T option
        ? ReturnIfRelevant(GetClassification(option, ctx))
        : Unhandled;

    private HandlerResult<InputShrinkingClassification> ReturnIfRelevant(InputShrinkingClassification classification)
        => classification is InputAssumedIrrelevant
            ? Unhandled
            : HandlerResult.Handled(classification);

    protected abstract InputShrinkingClassification GetClassification(T input, CustomShrinkingContext customContext);
}
