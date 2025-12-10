using QuickCheckr.UnderTheHood.Docketing;
using QuickCheckr.UnderTheHood.Memories.Structure.Lensing;

namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Collections;

public class ShrinkEachElementStrategy
{
    public Dictionary<int, int>? Mapper { get; set; }

    private int GetMappedIndex(int index) => Mapper == null ? index : Mapper[index];

    public IEnumerable<InputShrinkingClue> Shrink(
        CheckrState state,
        CheckrOf<Case> check,
        ShrinkingInfo info)
    {
        var theList = CloneList.AsOriginalType(info.value!);
        var values = new List<string>();
        int index = 0;
        var shrinkCount = 0;
        while (index < theList.Count)
        {
            var ix = index;
            var before = theList[ix];
            var valueType = before!.GetType();
            var elementType = valueType;
            using (state.Memory.ScopedSwap(info.executionId, info.key, theList))
            {
                var swapper = new ListLens(elementType, ix);
                using (state.Memory.NestedValue(info.executionId, swapper))
                {
                    var childInfo = info with { value = before, fullKey = $"{info.fullKey}.{GetMappedIndex(index)}" };
                    var clues = ShrinkStrategyPicker.Input(state, check, childInfo);
                    var removeForShrinking = clues.Any(a => a.Intent == ShrinkingIntent.Ignore);
                    shrinkCount += clues.Sum(a => a.Count);
                    if (removeForShrinking)
                    {
                        values.Add("_");
                        theList[ix] = state.GetSubstitute(elementType, before);
                    }
                    else
                    {
                        values.Add(clues.First().Report!);
                        theList[ix] = before;
                    }
                    index++;
                }
            }
        }
        var finalClue = InputShrinkingClue
            .Replace<ShrinkEachElementStrategy>(info.executionId, info.fullKey, info.value, theList, $"[ {string.Join(", ", values)} ]");
        finalClue.AddToCount(shrinkCount);
        state.Docket.Record(new InputShrinkingClueFound(finalClue));
        return [finalClue];
    }
}
