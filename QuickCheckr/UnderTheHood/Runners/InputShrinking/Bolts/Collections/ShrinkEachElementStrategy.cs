using QuickCheckr.UnderTheHood.Docketing;
using QuickCheckr.UnderTheHood.Memories.Structure.Lensing;

namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Collections;

public class ShrinkEachElementStrategy : IShrinkingStrategy
{
    public Dictionary<int, int>? Mapper { get; set; }

    private int GetMappedIndex(int index) => Mapper == null ? index : Mapper[index];

    public IEnumerable<InputShrinkingClue> Shrink(
        CheckrState state,
        CheckrOf<Case> check,
        ShrinkingInfo info)
    {
        var theList = CloneList.AsOriginalType(info.Value!);
        var values = new List<string>();
        int index = 0;
        var shrinkCount = 0;
        while (index < theList.Count)
        {
            var ix = index;
            var before = theList[ix];
            var valueType = before!.GetType();
            var elementType = valueType;
            var substitute = CloneList.GetSubstituteForType(info.Type, theList);
            using (state.Memory.ScopedSwap(info.ExecutionId, info.Key, substitute))
            {
                var swapper = new ListLens(elementType, ix);
                using (state.Memory.NestedValue(info.ExecutionId, swapper))
                {
                    var childInfo = info with { Value = before, FullKey = $"{info.FullKey}.{GetMappedIndex(index)}", Type = elementType };
                    var clues = new ShrinkStrategyPicker().Shrink(state, check, childInfo);
                    var removeForShrinking = clues.Any(a => a.Intent == ShrinkingIntent.Ignore);
                    shrinkCount += clues.Sum(a => a.Count);
                    if (removeForShrinking)
                    {
                        values.Add("_");
                        theList[ix] = before;
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
            .Replace<ShrinkEachElementStrategy>(info.ExecutionId, info.FullKey, info.Value, theList, $"[ {string.Join(", ", values)} ]");
        finalClue.AddToCount(shrinkCount);
        state.Docket.Record(new InputShrinkingClueFound(finalClue));
        return [finalClue];
    }
}
