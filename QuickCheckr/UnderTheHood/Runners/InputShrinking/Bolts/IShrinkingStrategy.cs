namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts;

public interface IShrinkingStrategy
{
    IEnumerable<InputShrinkingClue> Shrink(CheckrState state, CheckrOf<Case> check, ShrinkingInfo info);
}
