using QuickCheckr.UnderTheHood.Docketing;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts;

namespace QuickCheckr.UnderTheHood.Runners.InputShrinking;

public class InputShrinker(
    CheckrState State,
    CheckrOf<Case> Check)
{
    public int Run()
    {
        State.Docket.Record(new InputShrinkingStarted(State.ShortTermMemory.Count()));
        var shrinkCount = 0;
        foreach (var executionId in State.ShortTermMemory.Numbers)
        {
            State.ShortTermMemory.GotoExecution(executionId);
            var inputs = State.ShortTermMemory.GetInputs();
            foreach (var (key, value) in inputs)
            {
                State.Docket.Record(new InputShrinkRunStarted(executionId, key));
                var collector = new List<InputShrinkingClue>();
                ShrinkStrategyPicker.Input(State, Check, executionId, key, value, key);
                State.Docket.Record(new InputShrinkRunStarted(executionId, key));
            }
        }
        State.Docket.Record(new InputShrinkingEnded(State.ShortTermMemory.Count()));
        return shrinkCount;
    }
}
