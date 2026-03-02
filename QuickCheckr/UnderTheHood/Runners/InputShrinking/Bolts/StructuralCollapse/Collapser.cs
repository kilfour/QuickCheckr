using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Records;
using QuickCheckr.UnderTheHood.Warnings;

namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.StructuralCollapse;

public class Collapser(
    CheckrState State,
    CheckrOf<Case> Check,
    bool useDefaultReducers)
{
    public int Run()
    {
        var shrinkCount = 0;
        foreach (var (executionId, executionMemory) in State.Memory.Entries())
        {
            var inputs = executionMemory.GetInputs();
            var hasAtLeastOne = inputs.Any();
            var keptAtLeastOne = false;
            foreach (var (key, value) in inputs)
            {
                var info = new ShrinkingInfo(executionId, key, value.Value, key, value.Type);
                var classification = new CollapseSelector().Classify(info, new ShrinkingContext(State, Check, useDefaultReducers));
                shrinkCount += IrrelevantCount.From(classification);
                executionMemory.StoreInputShrinkingClassification(key, classification);
                if (classification is InputRelevant)
                {
                    keptAtLeastOne = true;
                }
                keptAtLeastOne = keptAtLeastOne || State.Memory.HasClassifier(info.ExecutionId, info.Key, info.Type);
                var (unused, types) = executionMemory.HasUnusedClassifiers(key);
                if (unused)
                {
                    var noun = types.Count == 1 ? "shrinker" : "shrinkers";
                    executionMemory.StoreWarning($"UnusedShrinkers-{executionId}", () => new UnusedShrinkerWarning(types, key));
                }
            }
            if (hasAtLeastOne && !keptAtLeastOne)
                executionMemory.StoreWarning($"AllInputsShrunk-{executionId}", () => new AllInputsIrrelevantWarning());
        }
        return shrinkCount;
    }
}
