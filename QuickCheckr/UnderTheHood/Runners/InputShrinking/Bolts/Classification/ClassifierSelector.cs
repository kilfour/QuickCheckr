using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Custom;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Defaults;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Records;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Collapsing;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Traversal;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.InputReduction;

namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification;

internal sealed class ClassifierSelector
{
    public InputClassification Classify(ShrinkingInfo info, ShrinkingContext context)
    {
        info = CollapserSelector.Collapse(info, context);
        InputClassification? classification = null;
        var shrinkers = context.State.Memory.GetShrinkers(info.ExecutionId, info.Key, info.Type);
        if (shrinkers is not null)
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
            else
            {
                var traversal = shrinkers
                    .Where(shrinker => shrinker.Traversals.Length != 0)
                    .FirstOrDefault(shrinker => shrinker.Condition(info));
                if (traversal is not null)
                {
                    classification = TraversalSelector.Traverse(
                        info,
                        context,
                        traversal.Traversals);
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
