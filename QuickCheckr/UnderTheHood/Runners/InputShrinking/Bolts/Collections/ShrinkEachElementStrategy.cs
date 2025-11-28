using QuickCheckr.UnderTheHood.Docketing;
using QuickCheckr.UnderTheHood.Memories.Structure.Lensing;

namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Collections;

public class ShrinkEachElementStrategy
{
    public Dictionary<int, int>? Mapper { get; set; }

    private int GetMappedIndex(int index) => Mapper == null ? index : Mapper[index];

    public IEnumerable<InputShrinkingClue> Shrink<T>(
        CheckrState state,
        CheckrOf<Case> check,
        int executionId,
        string key,
        T value,
        string fullKey, int incomingShrinkCount)
    {
        var theList = CloneList.AsOriginalType(value!);
        var values = new List<string>();
        int index = 0;
        var shrinkCount = incomingShrinkCount;
        while (index < theList.Count)
        {
            var ix = index;
            var before = theList[ix];
            var valueType = before!.GetType();
            var elementType = valueType;
            using (state.Memory.ScopedSwap(executionId, key, theList))
            {
                var swapper = new ListLens(elementType, ix);
                using (state.Memory.NestedValue(executionId, swapper))
                {
                    var clues = ShrinkStrategyPicker.Input(state, check, executionId, key, before, $"{fullKey}.{GetMappedIndex(index)}");
                    var removeForShrinking = clues.Any(a => a.Intent == ShrinkingIntent.Ignore);
                    shrinkCount += clues.Sum(a => a.Count);
                    if (removeForShrinking)
                    {
                        values.Add("_");
                        theList[ix] = GetPlaceholder(theList[ix]!);
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
            .Replace<ShrinkEachElementStrategy>(executionId, fullKey, value, theList, $"[ {string.Join(", ", values)} ]");
        finalClue.AddToCount(shrinkCount);
        state.Docket.Record(new InputShrinkingClueFound(finalClue));
        return [finalClue];
    }

    private static object GetPlaceholder(object original)
    {
        var type = original.GetType();
        return type.IsValueType ? Activator.CreateInstance(type)! : null!;
    }
}
