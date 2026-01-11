using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts;

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
            foreach (var (key, clue) in shrinkOutcomes)
            {
                var reducerFactory = shortTermMemory.GetReducer(key);
                if (reducerFactory == null) continue;
                var reducer = reducerFactory();
                var verifier = new Verifier(State, Check);
                var result = new ReductionResult(true, clue.Result!);
                Reduction step = Reduction.Next(clue.Result!);
                while (step.Continue)
                {
                    if (verifier.RunFailed(executionId, key, step.Candidate!))
                        result = new ReductionResult(true, step.Candidate!);
                    else
                        result = new ReductionResult(false, result.Value!);
                    step = reducer(result);
                }
                shrinkCount++;
                shortTermMemory.StoreInputRedux(key, step.Result!);
            }
        }
        return shrinkCount;
    }
}
