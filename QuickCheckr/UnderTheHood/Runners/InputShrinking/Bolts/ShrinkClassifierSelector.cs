using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Custom;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Defaults;

namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts;

public class ShrinkClassifierSelector
{
    public InputShrinkingClassification Classify(ShrinkingInfo info, ShrinkingContext context)
    {
        var shrinkOptions = context.State.Memory.GetShrinkOptions(info.ExecutionId, info.Key, info.Type);
        if (shrinkOptions.Any())
            return ClassifyCustom(info, shrinkOptions, context);
        return ClassifyDefault(info, context);
    }

    private InputShrinkingClassification ClassifyCustom(ShrinkingInfo info, IEnumerable<IShrinkOption> options, ShrinkingContext context)
        => new CustomClassifierDispatcher(new DefaultClassifierDispatcher()).Dispatch(options, new CustomShrinkingContext(info, context));

    // TODO: private readonly DefaultClassifier defaultClassifier = new();

    public InputShrinkingClassification ClassifyDefault(ShrinkingInfo info, ShrinkingContext context)
        => new DefaultClassifierDispatcher().Dispatch(info, context);
}
