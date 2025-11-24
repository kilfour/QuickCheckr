using QuickCheckr.UnderTheHood.Docketing;
using QuickPulse.Show;

namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Collections;

public class RemoveOneByOneStrategy
{
    public IEnumerable<InputShrinkingClue> Shrink<T>(CheckrState state, CheckrOf<Case> check, int executionId, string key, T value, string fullKey)
    {
        var verifier = new Verifier(state, check);
        var theList = CloneList.AsOriginalType(value!);
        int index = 0;
        int indexKey = -1;
        while (index < theList.Count)
        {
            indexKey++;
            var ix = index;
            var before = theList[ix];
            theList.RemoveAt(ix);
            if (verifier.RunFailed(executionId, key, theList!))
            {
                var clue = new InputShrinkingClue
                {
                    ExecutionId = executionId,
                    Key = $"{fullKey}.{indexKey}",
                    Original = value,
                    Result = theList,
                    Report = null,
                    Intent = ShrinkingIntent.Remove,
                    Reason = null,
                    Strategy = typeof(RemoveOneByOneStrategy).Name
                };
                state.Docket.Record(new InputShrinkingClueFound(clue));
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
            var finalClue = new InputShrinkingClue
            {
                ExecutionId = executionId,
                Key = fullKey,
                Original = value,
                Result = theList,
                Report = Introduce.This(theList, false),
                Intent = ShrinkingIntent.Keep,
                Reason = null,
                Strategy = typeof(RemoveOneByOneStrategy).Name
            };
            state.Docket.Record(new InputShrinkingClueFound(finalClue));
            return [finalClue];
        }
        else
        {
            var finalClue = new InputShrinkingClue
            {
                ExecutionId = executionId,
                Key = fullKey,
                Original = value,
                Result = null,
                Report = null,
                Intent = ShrinkingIntent.Ignore,
                Reason = null,
                Strategy = typeof(RemoveOneByOneStrategy).Name
            };
            state.Docket.Record(new InputShrinkingClueFound(finalClue));
            return [finalClue];
        }
    }
}
