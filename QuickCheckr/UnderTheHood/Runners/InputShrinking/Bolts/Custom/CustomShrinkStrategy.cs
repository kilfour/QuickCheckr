

using QuickCheckr.UnderTheHood.Docketing;
using QuickPulse.Show;

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
                var clue = new InputShrinkingClue
                {
                    ExecutionId = executionId,
                    Key = key,
                    Original = value,
                    Result = value,
                    Report = Introduce.This(value!, false),
                    Intent = ShrinkingIntent.Keep,
                    Reason = candidate
                };
                state.Docket.Record(new InputShrinkingClueFound(clue));
                yield return clue;
            }
        }
    }
}