using QuickCheckr.UnderTheHood.Dispatching;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Defaults;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Records;

namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Custom;

internal sealed class CustomClassifierDispatcher(DefaultClassifierDispatcher defaults)
{
    private readonly IReadOnlyList<FirstMatchHandler<IClassificationOption, CustomShrinkingContext, InputClassification>> handlers =
            [ new CustomFuzzrShrinkClassifier()
            , new CustomFunctionShrinkClassifier()
            , new CustomDefaultShrinkClassifier(defaults)
            , new CustomKeepShrinkClassifier()
            , new CustomNoneShrinkClassifier()
            ];

    public async ValueTask<InputClassification> DispatchAsync(
        IEnumerable<IClassificationOption> options,
        CustomShrinkingContext context)
    {
        InputAssumedIrrelevant? irrelevant = null;
        foreach (var option in options)
        {
            foreach (var handler in handlers)
            {
                var result = await handler.TryHandleAsync(option, context).ConfigureAwait(false);
                if (!result.HasOutput)
                    continue;
                if (result.Output is InputAssumedIrrelevant assumedIrrelevant)
                {
                    irrelevant = assumedIrrelevant;
                    break;
                }
                return result.Output;
            }
        }
        return irrelevant is not null ? irrelevant : new CustomFallbackClassifier().Handle(context);
    }
}
