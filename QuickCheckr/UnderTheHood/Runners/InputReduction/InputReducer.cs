using QuickCheckr.UnderTheHood.Docketing;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts;
using QuickPulse.Show;

namespace QuickCheckr.UnderTheHood.Runners.InputReduction;

public class InputReducer(
    CheckrState State,
    CheckrOf<Case> Check)
{
    public int Run()
    {
        var shrinkCount = 0;
        foreach (var (executionId, shortTermMemory) in State.Memory.Entries())
        {

            var shrinkOutcomes = shortTermMemory.GetInputShrinkOutcomes();
            foreach (var (key, value) in shrinkOutcomes)
            {
                var reducer = shortTermMemory.GetReducer<object>(key);
                if (reducer == null) continue;
                var clue = value as InputShrinkingClue;
                var clues = Reduce(executionId, key, clue?.Result!, reducer(clue?.Result!));
                var replace = clues.FirstOrDefault(a => a.Intent == ShrinkingIntent.Replace);
                if (replace != null)
                {
                    State.Docket.Record(new InputReduced(executionId, key));
                    shortTermMemory.StoreInputShrinkOutcome(key, replace);
                }
            }
        }
        return shrinkCount;
    }

    public IEnumerable<InputShrinkingClue> Reduce(int executionId, string key, object value, IEnumerable<object> values)
    {
        var verifier = new Verifier(State, Check);
        var lastCandidate = value;
        foreach (var candidate in values.Where(x => !Equals(x, value)))
        {
            if (verifier.RunPassed(executionId, key, candidate!))
            {
                var clue = new InputShrinkingClue
                {
                    ExecutionId = executionId,
                    Key = key,
                    Original = value,
                    Result = lastCandidate,
                    Report = Introduce.This(lastCandidate!, false),
                    Intent = ShrinkingIntent.Replace,
                    Reason = lastCandidate,
                    Strategy = typeof(InputReducer).Name
                };
                State.Docket.Record(new InputShrinkingClueFound(clue));
                yield return clue;
            }
            lastCandidate = candidate;
        }
    }
}
