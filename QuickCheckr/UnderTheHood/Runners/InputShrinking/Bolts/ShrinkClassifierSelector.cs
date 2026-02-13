using QuickCheckr.UnderTheHood.Docketing;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Custom;
using QuickFuzzr;

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

    private InputShrinkingClassification ClassifyCustom2(ShrinkingInfo info, IEnumerable<IShrinkOption> options, ShrinkingContext context)
    {
        return new InputAssumedIrrelevant();
    }

    private InputShrinkingClassification ClassifyCustom(ShrinkingInfo info, IEnumerable<IShrinkOption> options, ShrinkingContext context)
    {
        foreach (var option in options)
        {
            if (option is IShrinkOptionFuzzr optionFuzzr)
            {
                var shrinker = optionFuzzr.GetFuzzr().Unique(Guid.NewGuid())
                    .Many(optionFuzzr.Times)(context.State.FuzzrState).Value.Cast<object>();
                var classification = new CustomShrinkClassifier(a => shrinker).Classify(info, context);
                if (classification is not InputAssumedIrrelevant)
                    return classification;
            }

            if (option is IShrinkOptionFunction optionCustom)
            {
                var classification = new CustomShrinkClassifier(optionCustom.GetFunction()).Classify(info, context);
                if (classification is not InputAssumedIrrelevant)
                    return classification;
            }

            if (option is IShrinkOptionDefault)
            {
                var classification = ClassifyDefault(info, context);
                if (classification is not InputAssumedIrrelevant)
                    return classification;
            }

            if (option is IShrinkOptionKeep)
            {
                var keep = InputShrinkingClue.Keep<CustomShrinkClassifier>(
                info.ExecutionId, info.Path, info.Value, context.State.InputPrinter.IntroduceThis(info.Value));
                context.State.Docket.Record(new InputShrinkingClueFound(keep));
                return new InputRelevant(context.State.InputPrinter.IntroduceThis(info.Value));
            }

            if (option is IShrinkOptionNone) return new Unclassified();
        }
        var clue = InputShrinkingClue.Ignore<CustomShrinkClassifier>(info.ExecutionId, info.Path, info.Value);
        context.State.Docket.Record(new InputShrinkingClueFound(clue));
        return new InputAssumedIrrelevant();
    }

    // TODO: private readonly DefaultClassifier defaultClassifier = new();

    public InputShrinkingClassification ClassifyDefault(ShrinkingInfo info, ShrinkingContext context)
        => new DefaultClassifier().Dispatch(info, context);
}
