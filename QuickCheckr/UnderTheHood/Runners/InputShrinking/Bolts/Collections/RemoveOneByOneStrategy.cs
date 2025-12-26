using QuickCheckr.UnderTheHood.Docketing;

namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Collections;

public class RemoveOneByOneStrategy : IShrinkingStrategy
{
    public IEnumerable<InputShrinkingClue> Shrink(CheckrState state, CheckrOf<Case> check, ShrinkingInfo info)
    {
        var verifier = new Verifier(state, check);
        var theList = CloneList.AsOriginalType(info.Value!);
        var elementCount = theList.Count;
        int index = 0;
        int indexKey = -1;
        var shrinkCount = 0;
        while (index < theList.Count)
        {
            indexKey++;
            var ix = index;
            var before = theList[ix];
            theList.RemoveAt(ix);
            var substitute = CloneList.GetSubstituteForType(info.Type, theList);
            if (verifier.RunFailed(info.ExecutionId, info.Key, substitute!))
            {
                var clue = InputShrinkingClue.Remove<RemoveOneByOneStrategy>(info.ExecutionId, $"{info.FullKey}.{indexKey}", info.Value, theList);
                state.Docket.Record(new InputShrinkingClueFound(clue));
                shrinkCount++;
                continue;
            }
            theList.Insert(ix, before);
            index++;
        }

        if (elementCount == 0 || elementCount != shrinkCount)
        {
            var finalClue = InputShrinkingClue.Keep<RemoveOneByOneStrategy>(info.ExecutionId, info.FullKey, info.Value, theList);
            finalClue.AddToCount(shrinkCount);
            state.Docket.Record(new InputShrinkingClueFound(finalClue));
            return [finalClue];
        }
        else
        {
            var finalClue = InputShrinkingClue.Ignore<RemoveOneByOneStrategy>(info.ExecutionId, info.FullKey, info.Value);
            state.Docket.Record(new InputShrinkingClueFound(finalClue));
            return [finalClue];
        }
    }
}
