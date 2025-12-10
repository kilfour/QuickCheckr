using QuickCheckr.UnderTheHood.Docketing;

namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Collections;

public class RemoveOneByOneStrategy
{
    public IEnumerable<InputShrinkingClue> Shrink<T>(CheckrState state, CheckrOf<Case> check, int executionId, string key, T value, string fullKey)
    {
        var verifier = new Verifier(state, check);
        var theList = CloneList.AsOriginalType(value!);
        var elementCount = theList.Count;
        int index = 0;
        int indexKey = -1;
        var shrinkCount = 0;
        var elementType = typeof(object);
        while (index < theList.Count)
        {
            indexKey++;
            var ix = index;
            var before = theList[ix];
            elementType = before == null ? typeof(object) : before.GetType()!;
            theList.RemoveAt(ix);
            // ------------------------------------------------------------------------------
            // -- HACKED IN AN ARRAY
            if (value != null)
            {
                var valueType = value.GetType();
                if (value.GetType().IsArray)
                {
                    elementType = valueType.GetElementType()!;
                    var array = Array.CreateInstance(elementType, theList.Count);
                    for (var i = 0; i < theList.Count; i++)
                    {
                        array.SetValue(theList[i], i);
                    }
                    if (verifier.RunFailed(executionId, key, array))
                    {
                        var clue = InputShrinkingClue.Remove<RemoveOneByOneStrategy>(executionId, $"{fullKey}.{indexKey}", value, theList);
                        state.Docket.Record(new InputShrinkingClueFound(clue));
                        shrinkCount++;
                        continue;
                    }
                }
            }
            // ------------------------------------------------------------------------------
            if (verifier.RunFailed(executionId, key, theList!))
            {
                var clue = InputShrinkingClue.Remove<RemoveOneByOneStrategy>(executionId, $"{fullKey}.{indexKey}", value, theList);
                state.Docket.Record(new InputShrinkingClueFound(clue));
                shrinkCount++;
                continue;
            }
            theList.Insert(ix, before);
            index++;
        }

        if (elementCount == 0 || elementCount != shrinkCount)
        {
            var finalClue = InputShrinkingClue.Keep<RemoveOneByOneStrategy>(executionId, fullKey, value, theList);
            finalClue.AddToCount(shrinkCount);
            state.Docket.Record(new InputShrinkingClueFound(finalClue));
            return [finalClue];
        }
        else
        {
            var finalClue = InputShrinkingClue.Ignore<RemoveOneByOneStrategy>(executionId, fullKey, value);
            state.Docket.Record(new InputShrinkingClueFound(finalClue));
            return [finalClue];
        }
    }
}
