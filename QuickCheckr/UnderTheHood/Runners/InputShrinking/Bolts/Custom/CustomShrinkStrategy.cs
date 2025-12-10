

using QuickCheckr.UnderTheHood.Docketing;

namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Custom;

public class CustomShrinkStrategy(Func<object, IEnumerable<object>> shrinker) : IShrinkingStrategy
{
    public IEnumerable<InputShrinkingClue> Shrink(CheckrState state, CheckrOf<Case> check, ShrinkingInfo info)
    {
        var verifier = new Verifier(state, check);
        var values = shrinker(info.value!);
        foreach (var candidate in values.Where(x => !Equals(x, info.value)))
        {
            if (verifier.RunPassed(info.executionId, info.key, candidate!))
            {
                var clue = InputShrinkingClue.Keep<CustomShrinkStrategy>(
                    info.executionId, info.fullKey, info.value, state.Editor.IntroduceThis(info.value));
                state.Docket.Record(new InputShrinkingClueFound(clue));
                return [clue];
            }
        }
        var ignore = InputShrinkingClue.Ignore<CustomShrinkStrategy>(info.executionId, info.fullKey, info.value);
        state.Docket.Record(new InputShrinkingClueFound(ignore));
        return [ignore];
    }
}