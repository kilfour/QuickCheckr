using QuickCheckr.UnderTheHood.Docketing;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Custom;
using QuickFuzzr;

namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts;

public class ShrinkStrategyPicker : IShrinkingStrategy
{
    public IEnumerable<InputShrinkingClue> Shrink(CheckrState state, CheckrOf<Case> check, ShrinkingInfo info)
    {
        //var opt = state.Memory.GetShrinker(info.ExecutionId, info.Key, info.Type);
        var options = state.Memory.GetShrinker(info.ExecutionId, info.Key, info.Type);
        if (options is not null && options.Any())
        {
            //for (int i = 0; i < options.Length; i++)
            foreach (var option in options)
            {
                if (option is IShrinkOptionFuzzr optionFuzzr)
                {
                    var shrinker = optionFuzzr.GetFuzzr().Unique(Guid.NewGuid())
                        .Many(optionFuzzr.Times)(state.FuzzrState).Value.Cast<object>();
                    var clues = new CustomShrinkStrategy(a => shrinker).Shrink(state, check, info);
                    if (clues.Any())
                        return clues;
                }

                if (option is IShrinkOptionFunction optionCustom)
                {
                    var clues = new CustomShrinkStrategy(optionCustom.GetFunction()).Shrink(state, check, info);
                    if (clues.Any())
                        return clues;
                }

                if (option is IShrinkOptionDefault)
                {
                    var clues = new ShrinkStrategyPickerDefault().Shrink(state, check, info);
                    if (clues.Any(a => a.Strategy != "CantShrinkNull"))
                        return clues;
                }
            }
            var clue = InputShrinkingClue.Keep<CustomShrinkStrategy>(
                    info.ExecutionId, info.FullKey, info.Value, state.InputPrinter.IntroduceThis(info.Value));
            state.Docket.Record(new InputShrinkingClueFound(clue));
            return [clue];
        }
        else
            return new ShrinkStrategyPickerDefault().Shrink(state, check, info);
    }
}
