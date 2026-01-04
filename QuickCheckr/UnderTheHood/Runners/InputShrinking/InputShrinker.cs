using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts;

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
            }
            if (hasAtLeastOne && !keptAtLeastOne)
                executionMemory.StoreWarning($"AllInputsShrunk-{executionId}", () => "All inputs were considered irrelevant.");
        }
        return shrinkCount;
    }
}
