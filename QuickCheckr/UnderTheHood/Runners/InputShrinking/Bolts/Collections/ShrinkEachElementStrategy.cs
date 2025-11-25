using System.Collections;
using QuickCheckr.UnderTheHood.Docketing;
using QuickCheckr.UnderTheHood.Memories;

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
            // var elementType = valueType.IsGenericType
            //     ? valueType.GetGenericArguments().First()
            //     : typeof(object);
            using (state.Memory.ShortTermMemory.GetMemory(executionId).ScopedSwap(key, theList))
            {
                var swapper = new MemoryLens(
                    list => ((IList)list)[ix]!,
                    (list, element) =>
                    {
                        if (element == null || elementType.IsAssignableFrom(element.GetType()))
                            ((IList)list)[ix] = element; // NESTED LIST do not get here
                        return list;
                    });
                using (state.Memory.ShortTermMemory.GetMemory(executionId).NestedValue(swapper))
                {
                    var clues = ShrinkStrategyPicker.Input(state, check, executionId, key, before, $"{fullKey}.{GetMappedIndex(index)}");
                    var removeForShrinking = !clues.Any();
                    // state.Memory.AccessForForThisExecution().GetDecorated(key).GetShrinkTraces()
                    //     .Any(a => a.Key == $"{fullKey}.{GetMappedIndex(index)}" && a.IsIrrelevant);
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
        // var shrinkValues = Children.Values.Select(a => a.GetShrinkReportString("_")).Where(a => !string.IsNullOrEmpty(a));
        //     if (shrinkValues.Any())
        //         return $"[ {string.Join(", ", shrinkValues)} ]";
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
