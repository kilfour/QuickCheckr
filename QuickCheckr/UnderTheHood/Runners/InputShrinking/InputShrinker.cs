using QuickCheckr.Diagnostics.Bolts;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Records;
using QuickCheckr.UnderTheHood.Warnings;

namespace QuickCheckr.UnderTheHood.Runners.InputShrinking;

internal sealed class InputShrinker(
    CheckrState State,
    CheckrOf<Case> Check,
    bool useDefaultReducers)
{
    public int Run()
    {
        using var _ = State.Pathologist.Examine(AutopsyPath.InputShrinking);
        var shrinkCount = 0;
        foreach (var (executionId, executionMemory) in State.Memory.Entries())
        {
            using var __ = State.Pathologist.Examine(AutopsyPath.Checking(executionId));
            var inputs = executionMemory.GetInputs();
            var hasAtLeastOne = inputs.Any();
            var keptAtLeastOne = false;
            foreach (var (key, value) in inputs)
            {
                using var ___ = State.Pathologist.Examine(AutopsyPath.Input(key));
                var info = new ShrinkingInfo(executionId, key, value.Value, key, value.Type);
                var context = new ShrinkingContext(State, Check, useDefaultReducers);
                var classification = new ClassifierSelector().Classify(info, context);
                shrinkCount += context.CollapseCount + IrrelevantCount.From(classification);
                executionMemory.StoreInputShrinkingClassification(key, classification);
                if (classification is InputRelevant)
                {
                    keptAtLeastOne = true;
                }
                keptAtLeastOne = keptAtLeastOne || State.Memory.HasClassifier(info);
                var (unused, types) = executionMemory.HasUnusedClassifiers(key);
                if (unused)
                {
                    executionMemory.StoreWarning($"UnusedShrinkers-{executionId}", () => new UnusedShrinkerWarning(types, key));
                }
            }
            if (hasAtLeastOne && !keptAtLeastOne)
                executionMemory.StoreWarning($"AllInputsShrunk-{executionId}", () => new AllInputsIrrelevantWarning());
        }
        return shrinkCount;
    }
}
