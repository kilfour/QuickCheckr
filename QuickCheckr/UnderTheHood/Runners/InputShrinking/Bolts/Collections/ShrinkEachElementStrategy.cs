using System.Collections;
using QuickCheckr.UnderTheHood.Docketing;
using QuickCheckr.UnderTheHood.Memories.Structure;

namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Collections;

public class ShrinkEachElementStrategy
{
    public Dictionary<int, int>? Mapper { get; set; }

    private int GetMappedIndex(int index) => Mapper == null ? index : Mapper[index];

    public IEnumerable<InputShrinkingClue> Shrink<T>(CheckrState state, CheckrOf<Case> check, int executionId, string key, T value, string fullKey)
    {
        var theList = CloneList.AsOriginalType(value!);
        var values = new List<string>();
        int index = 0;
        while (index < theList.Count)
        {
            var ix = index;
            var before = theList[ix];
            var valueType = before!.GetType();
            var elementType = valueType;
            using (state.Memory.ScopedSwap(executionId, key, theList))
            {
                var swapper = new MemoryLens(
                    list => ((IList)list)[ix]!,
                    (list, element) =>
                    {
                        if (element == null || elementType.IsAssignableFrom(element.GetType()))
                            ((IList)list)[ix] = element;
                        return list;
                    });
                using (state.Memory.NestedValue(executionId, swapper))
                {
                    var clues = ShrinkStrategyPicker.Input(state, check, executionId, key, before, $"{fullKey}.{GetMappedIndex(index)}");
                    var removeForShrinking = !clues.Any();
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
        var finalClue = new InputShrinkingClue
        {
            ExecutionId = executionId,
            Key = fullKey,
            Original = value,
            Result = theList,
            Report = $"[ {string.Join(", ", values)} ]",
            Intent = ShrinkingIntent.Replace,
            Reason = null,
            Strategy = typeof(ShrinkEachElementStrategy).Name
        };
        state.Docket.Record(new InputShrinkingClueFound(finalClue));
        return [finalClue];
    }

    private static object GetPlaceholder(object original)
    {
        var type = original.GetType();
        return type.IsValueType ? Activator.CreateInstance(type)! : null!;
    }
}
