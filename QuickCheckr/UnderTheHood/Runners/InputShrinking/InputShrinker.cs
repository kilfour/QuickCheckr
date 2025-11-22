using QuickCheckr.UnderTheHood.Docketing;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts;

namespace QuickCheckr.UnderTheHood.Runners.InputShrinking;

public class InputShrinker(
    CheckrState State,
    CheckrOf<Case> Check)
{
    public int Run()
    {
        State.Docket.Record(new InputShrinkingStarted(State.ShortTermMemories.Count()));
        var shrinkCount = 0;
        foreach (var (executionId, shortTermMemory) in State.ShortTermMemories.Entries())
        {
            var inputs = shortTermMemory.GetInputs();
            foreach (var (key, value) in inputs)
            {
                State.Docket.Record(new InputShrinkRunStarted(executionId, key));
                var clues = ShrinkStrategyPicker.Input(State, Check, executionId, key, value, key);
                foreach (var clue in clues)
                {
                    if (clue.Intent == ShrinkingIntent.Keep)
                    {
                        shortTermMemory.StoreInputShrinkOutcome(clue.Key, clue.Result);
                        break;
                    }
                }
                State.Docket.Record(new InputShrinkRunEnded(executionId, key));
            }
        }
        State.Docket.Record(new InputShrinkingEnded(State.ShortTermMemories.Count()));
        return shrinkCount;
    }
}
