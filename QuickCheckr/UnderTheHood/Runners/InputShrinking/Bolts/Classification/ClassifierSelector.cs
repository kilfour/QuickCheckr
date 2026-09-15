using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Custom;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Defaults;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Records;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Collapsing;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Traversal;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.InputReduction;

namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification;

internal sealed class ClassifierSelector
{
    public async ValueTask<InputClassification> ClassifyAsync(ShrinkingInfo info, ShrinkingContext context)
    {
        info = await CollapserSelector.CollapseAsync(info, context).ConfigureAwait(false);
        InputClassification? classification = null;
        var shrinkers = context.State.Memory.GetShrinkers(info.ExecutionId, info.Key, info.Type);
        if (shrinkers is not null)
        {
            var classifier = shrinkers
                .Where(shrinker => shrinker.Classifiers.Length != 0)
                .FirstOrDefault(shrinker => shrinker.Condition(info));
            if (classifier is not null)
            {
                classification = await ClassifyCustomAsync(
                    info,
                    classifier.Classifiers,
                    context).ConfigureAwait(false);
            }
            else
            {
                var traversal = shrinkers
                    .Where(shrinker => shrinker.Traversals.Length != 0)
                    .FirstOrDefault(shrinker => shrinker.Condition(info));
                if (traversal is not null)
                {
                    classification = await TraversalSelector.TraverseAsync(
                        info,
                        context,
                        traversal.Traversals).ConfigureAwait(false);
                }
            }
        }
        classification ??= await ClassifyDefaultAsync(info, context).ConfigureAwait(false);
        info = info with
        {
            Value = context.State.Memory.GetCurrentInputValue(
                info.ExecutionId,
                info.Key)
        };
        classification = await ReducerSelector.ReduceAsync(info, context, classification).ConfigureAwait(false);
        return classification;
    }

    private async ValueTask<InputClassification> ClassifyCustomAsync(ShrinkingInfo info, IEnumerable<IClassificationOption> options, ShrinkingContext context)
        => await new CustomClassifierDispatcher(new DefaultClassifierDispatcher()).DispatchAsync(options, new CustomShrinkingContext(info, context)).ConfigureAwait(false);

    public async ValueTask<InputClassification> ClassifyDefaultAsync(ShrinkingInfo info, ShrinkingContext context)
        => await new DefaultClassifierDispatcher().DispatchAsync(info, context).ConfigureAwait(false);
}
