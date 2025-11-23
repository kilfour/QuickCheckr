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
                    Result = null,
                    Report = null,
                    Intent = ShrinkingIntent.Remove,
                    Reason = null
                };
                state.Docket.Record(new InputShrinkingClueFound(clue));
                continue;
            }
            // if (state.VerifyIf.RunFailed(key, theList!))
            // {
            //     state.CurrentExecutionContext().ShrinkTrace(key, ShrinkKind.KeepSameKind, new ShrinkTrace
            //     {
            //         ExecutionId = -1,
            //         Key = $"{fullKey}.{indexKey}",
            //         Name = indexKey.ToString(),
            //         Original = before,
            //         Result = null,
            //         Intent = ShrinkIntent.Remove,
            //         Strategy = "RemoveOneByOneStrategy"
            //     });
            //     continue;
            // }
            // state.CurrentExecutionContext().ShrinkTrace(key, ShrinkKind.KeepSameKind, new ShrinkTrace
            // {
            //     ExecutionId = -1,
            //     Key = $"{fullKey}.{indexKey}",
            //     Name = indexKey.ToString(),
            //     Original = before,
            //     Result = before,
            //     Intent = ShrinkIntent.Keep,
            //     Strategy = "RemoveOneByOneStrategy"
            // });
            theList.Insert(ix, before);
            index++;
        }
        var finalClue = new InputShrinkingClue
        {
            ExecutionId = executionId,
            Key = key,
            Original = value,
            Result = theList,
            Report = Introduce.This(theList, false),
            Intent = ShrinkingIntent.Keep,
            Reason = null
        };
        state.Docket.Record(new InputShrinkingClueFound(finalClue));
        return [finalClue];
    }
}
