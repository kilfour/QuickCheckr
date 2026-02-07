using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts;
using QuickCheckr.UnderTheHood.Warnings;

namespace QuickCheckr.UnderTheHood.Runners.InputShrinking;

public class InputShrinker(
    CheckrState State,
    CheckrOf<Case> Check)
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
                var clues = new ShrinkStrategyPicker().Shrink(State, Check, info).ToList();
                shrinkCount += clues.Sum(a => a.Count);
                var replace = clues.FirstOrDefault(a => a.Intent == ShrinkingIntent.Replace);
                if (replace != null)
                {
                    keptAtLeastOne = true;
                    executionMemory.StoreInputShrinkOutcome(key, replace);
                }
                else
                {
                    var keep = clues.FirstOrDefault(a => a.Intent == ShrinkingIntent.Keep);
                    if (keep != null)
                    {
                        keptAtLeastOne = true;
                        executionMemory.StoreInputShrinkOutcome(key, keep);
                    }
                }
                keptAtLeastOne = keptAtLeastOne || State.Memory.HasShrinker(info.ExecutionId, info.Key, info.Type);
                var (unused, types) = executionMemory.HasUnusedShrinkers(key);
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
