using QuickCheckr.UnderTheHood.Docketing;

namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Collections;

public class RemoveOneByOneStrategy : IShrinkingStrategy
{
    public IEnumerable<InputShrinkingClue> Shrink(CheckrState state, CheckrOf<Case> check, ShrinkingInfo info)
    {
        var verifier = new Verifier(state, check);
        var theList = CloneList.AsOriginalType(info.value!);
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
            if (info.value != null)
            {
                var valueType = info.value.GetType();
                if (info.value.GetType().IsArray)
                {
                    elementType = valueType.GetElementType()!;
                    var array = Array.CreateInstance(elementType, theList.Count);
                    for (var i = 0; i < theList.Count; i++)
                    {
                        array.SetValue(theList[i], i);
                    }
                    if (verifier.RunFailed(info.executionId, info.key, array))
                    {
                        var clue = InputShrinkingClue.Remove<RemoveOneByOneStrategy>(info.executionId, $"{info.fullKey}.{indexKey}", info.value, theList);
                        state.Docket.Record(new InputShrinkingClueFound(clue));
                        shrinkCount++;
                        continue;
                    }
                }
            }
            // ------------------------------------------------------------------------------
            if (verifier.RunFailed(info.executionId, info.key, theList!))
            {
                var clue = InputShrinkingClue.Remove<RemoveOneByOneStrategy>(info.executionId, $"{info.fullKey}.{indexKey}", info.value, theList);
                state.Docket.Record(new InputShrinkingClueFound(clue));
                shrinkCount++;
                continue;
            }
            theList.Insert(ix, before);
            index++;
        }

        if (elementCount == 0 || elementCount != shrinkCount)
        {
            var finalClue = InputShrinkingClue.Keep<RemoveOneByOneStrategy>(info.executionId, info.fullKey, info.value, theList);
            finalClue.AddToCount(shrinkCount);
            state.Docket.Record(new InputShrinkingClueFound(finalClue));
            return [finalClue];
        }
        else
        {
            var finalClue = InputShrinkingClue.Ignore<RemoveOneByOneStrategy>(info.executionId, info.fullKey, info.value);
            state.Docket.Record(new InputShrinkingClueFound(finalClue));
            return [finalClue];
        }
    }
}
