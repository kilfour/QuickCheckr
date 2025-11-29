

using QuickCheckr.UnderTheHood.Docketing;

namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Custom;

public class CustomShrinkStrategy<T>(Func<T, IEnumerable<T>> shrinker)
{
    public IEnumerable<InputShrinkingClue> Shrink(CheckrState state, CheckrOf<Case> check, int executionId, string key, T value, string fullKey)
    {
        var verifier = new Verifier(state, check);
        var values = shrinker(value!);
        foreach (var candidate in values.Where(x => !Equals(x, value)))
        {
            if (verifier.RunPassed(executionId, key, candidate!))
            {
                var clue = InputShrinkingClue.Keep<CustomShrinkStrategy<T>>(executionId, fullKey, value);
                state.Docket.Record(new InputShrinkingClueFound(clue));
                return [clue];
            }
        }
        var ignore = InputShrinkingClue.Ignore<CustomShrinkStrategy<T>>(executionId, fullKey, value);
        state.Docket.Record(new InputShrinkingClueFound(ignore));
        return [ignore];
    }
}