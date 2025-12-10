
namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Collections;

public class EnumerableShrinkStrategy : IShrinkingStrategy
{
    public IEnumerable<InputShrinkingClue> Shrink(CheckrState state, CheckrOf<Case> check, ShrinkingInfo info)
    {
        var clues = new RemoveOneByOneStrategy().Shrink(state, check, info);
        if (clues.Any(a => a.Intent == ShrinkingIntent.Ignore)) return clues;

        var keep = clues.First(a => a.Intent != ShrinkingIntent.Ignore);
        clues = clues.Concat(new ShrinkEachElementStrategy().Shrink(state, check, info with { value = keep.Result }));
        return clues;
    }
}
