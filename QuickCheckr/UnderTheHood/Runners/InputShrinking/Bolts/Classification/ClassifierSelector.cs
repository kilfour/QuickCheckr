using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Custom;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Defaults;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Records;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Collapsing;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.InputReduction;

namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification;

public class ClassifierSelector
{
    public InputClassification Classify(ShrinkingInfo info, ShrinkingContext context)
    {
        InputClassification? classification = null;
        var shrinkers = context.State.Memory.GetShrinkers(info.ExecutionId, info.Key, info.Type);
        if (shrinkers is not null)
        {
            var collapser = shrinkers
                .Where(shrinker => shrinker.Collapsers.Length != 0)
                .FirstOrDefault(shrinker => shrinker.Condition(info));
            if (collapser is not null)
            {
                classification = CollapserSelector.Collapse(
                    info,
                    context,
                    collapser.Collapsers);
            }
            else
            {
                var classifier = shrinkers
                    .Where(shrinker => shrinker.Classifiers.Length != 0)
                    .FirstOrDefault(shrinker => shrinker.Condition(info));
                if (classifier is not null)
                {
                    classification = ClassifyCustom(
                        info,
                        classifier.Classifiers,
                        context);
                }
            }
        }
        classification ??= ClassifyDefault(info, context);
        info = info with
        {
            Value = context.State.Memory.GetCurrentInputValue(
                info.ExecutionId,
                info.Key)
        };
        classification = ReducerSelector.Reduce(info, context, classification);
        return classification;
    }

    private InputClassification ClassifyCustom(ShrinkingInfo info, IEnumerable<IClassificationOption> options, ShrinkingContext context)
        => new CustomClassifierDispatcher(new DefaultClassifierDispatcher()).Dispatch(options, new CustomShrinkingContext(info, context));

    public InputClassification ClassifyDefault(ShrinkingInfo info, ShrinkingContext context)
        => new DefaultClassifierDispatcher().Dispatch(info, context);
}
