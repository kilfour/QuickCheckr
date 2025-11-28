using QuickCheckr.UnderTheHood.Docketing;

namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Collections;

public class RemoveOneByOneStrategy
{
    public IEnumerable<InputShrinkingClue> Shrink<T>(CheckrState state, CheckrOf<Case> check, int executionId, string key, T value, string fullKey)
    {
        var verifier = new Verifier(state, check);
        var theList = CloneList.AsOriginalType(value!);
        int index = 0;
        int indexKey = -1;
        var shrinkCount = 0;
        while (index < theList.Count)
        {
            indexKey++;
            var ix = index;
            var before = theList[ix];
            theList.RemoveAt(ix);
            if (verifier.RunFailed(executionId, key, theList!))
            {
                var clue = InputShrinkingClue.Remove<RemoveOneByOneStrategy>(executionId, $"{fullKey}.{indexKey}", value, theList);
                state.Docket.Record(new InputShrinkingClueFound(clue));
                shrinkCount++;
                continue;
            }
            // else
            // {
            //     var clue = new InputShrinkingClue
            //     {
            //         ExecutionId = executionId,
            //         Key = $"{fullKey}.{indexKey}",
            //         Original = value,
            //         Result = value,
            //         Report = null,
            //         Intent = ShrinkingIntent.Keep,
            //         Reason = null
            //     };
            //     state.Docket.Record(new InputShrinkingClueFound(clue));
            // }
            theList.Insert(ix, before);
            index++;
        }

        if (verifier.RunPassed(executionId, key, null!))
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
