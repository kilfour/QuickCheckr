
namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Collections;

public class EnumerableShrinkStrategy
{
    public IEnumerable<InputShrinkingClue> Shrink<T>(CheckrState state, CheckrOf<Case> check, int executionId, string key, T value, string fullKey)
    {
        // state.CurrentExecutionContext().ShrinkTrace(key, ShrinkKind.EnumerableKind, new ShrinkTrace
        // {
        //     ExecutionId = -1,
        //     Key = fullKey,
        //     Name = fullKey.Split(".").Last(),
        //     Original = value,
        //     Result = null,
        //     Intent = ShrinkIntent.Keep,
        //     Strategy = "EnumerableShrinkStrategy"
        // });
        // state.ShrinkingRegistry.GetCollectionStrategies()
        //     .ToList()
        //     .ForEach(a => { a.Shrink(state, key, value, fullKey); });
        var clues = new RemoveOneByOneStrategy().Shrink(state, check, executionId, key, value, fullKey);

        // new GreedyShrinkEachElementStrategy().Shrink(state, check, executionId, key, value, fullKey);
        var keep = clues.FirstOrDefault(a => a.Intent == ShrinkingIntent.Keep);
        if (keep != null)
        {
            clues = clues.Concat(new ShrinkEachElementStrategy().Shrink(state, check, executionId, key, keep.Result, fullKey));
        }
        else
            clues = clues.Concat(new ShrinkEachElementStrategy().Shrink(state, check, executionId, key, value, fullKey));
        return clues;
    }
}
