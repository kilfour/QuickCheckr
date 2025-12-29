using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Custom;
using QuickFuzzr;

namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts;

public class ShrinkStrategyPicker : IShrinkingStrategy
{
    public IEnumerable<InputShrinkingClue> Shrink(CheckrState state, CheckrOf<Case> check, ShrinkingInfo info)
    {
        var customShrinker = state.Memory.GetShrinker(info.ExecutionId, info.Key);
        if (customShrinker != null)
            return new CustomShrinkStrategy(customShrinker).Shrink(state, check, info);

        var customShrinkerr = state.Memory.GetShrinkerr(info.ExecutionId, info.Type.FullName!);
        if (customShrinkerr != null)
        {
            foreach (var option in customShrinkerr)
            {
                if (option is IShrinkOptionFuzzr optionFuzzr)
                {
                    var fuzzrShrinker = optionFuzzr.GetFuzzr().Unique(Guid.NewGuid())
                        .Many(optionFuzzr.Times)(state.FuzzrState).Value.Cast<object>();
                    return new CustomShrinkStrategy(a => fuzzrShrinker).Shrink(state, check, info);
                }
            }
        }
        return new ShrinkStrategyPickerDefault().Shrink(state, check, info);
    }
}
