using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts;

namespace QuickCheckr.UnderTheHood.Runners.InputShrinking;

public class InputShrinker(
    CheckrState State,
    CheckrOf<Case> Check)
{
    public int Run()
    {
        var shrinkCount = 0;
        foreach (var (executionId, shortTermMemory) in State.Memory.Entries())
        {
            var inputs = shortTermMemory.GetInputs();
            foreach (var (key, value) in inputs)
            {
                var clues = ShrinkStrategyPicker.Input(State, Check, executionId, key, value, key);
                var replace = clues.FirstOrDefault(a => a.Intent == ShrinkingIntent.Replace);
                if (replace != null)
                {
                    shortTermMemory.StoreInputShrinkOutcome(key, replace);
                }
                else
                {
                    var keep = clues.FirstOrDefault(a => a.Intent == ShrinkingIntent.Keep);
                    if (keep != null)
                    {
                        shortTermMemory.StoreInputShrinkOutcome(key, keep);
                    }
                }
            }
        }
        return shrinkCount;
    }
}
