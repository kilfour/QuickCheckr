
namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Collections;

public class EnumerableShrinkStrategy
{
    public IEnumerable<InputShrinkingClue> Shrink<T>(CheckrState state, CheckrOf<Case> check, int executionId, string key, T value, string fullKey)
    {
        var clues = new RemoveOneByOneStrategy().Shrink(state, check, executionId, key, value, fullKey);
        if (clues.Any(a => a.Intent == ShrinkingIntent.Ignore)) return clues;
        var keep = clues.FirstOrDefault(a => a.Intent != ShrinkingIntent.Ignore);
        var shrinkCount = clues.Sum(a => a.Count);
        if (keep != null)
        {
            clues = clues.Concat(new ShrinkEachElementStrategy().Shrink(state, check, executionId, key, keep.Result, fullKey, shrinkCount));
        }
        else
            clues = clues.Concat(new ShrinkEachElementStrategy().Shrink(state, check, executionId, key, value, fullKey, shrinkCount));
        return clues;
    }
}
