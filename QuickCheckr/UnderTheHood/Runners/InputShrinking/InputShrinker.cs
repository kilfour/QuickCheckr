using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Records;
using QuickCheckr.UnderTheHood.Warnings;

namespace QuickCheckr.UnderTheHood.Runners.InputShrinking;

public class InputShrinker(
    CheckrState State,
    CheckrOf<Case> Check)
{
    public int Run()
    {
        var shrinkCount = 0;
        foreach (var (executionId, executionMemory) in State.Memory.Entries())
        {
            var inputs = executionMemory.GetInputs();
            var hasAtLeastOne = inputs.Any();
            var keptAtLeastOne = false;
            foreach (var (key, value) in inputs)
            {
                var info = new ShrinkingInfo(executionId, key, value.Value, key, value.Type);
                var classification = new ShrinkClassifierSelector().Classify(info, new ShrinkingContext(State, Check));
                shrinkCount += IrrelevantCount.From(classification);
                executionMemory.StoreInputShrinkingClassification(key, classification);
                if (classification is InputRelevant)
                {
                    keptAtLeastOne = true;
                    // var reducerFactory = executionMemory.GetReducer(key);
                    // if (reducerFactory != null)
                    // {
                    //     var reducer = reducerFactory();
                    //     var verifier = new Verifier(State, Check);
                    //     var result = new ReductionResult(true, value.Value);
                    //     Reduction step = Reduction.Next(value.Value!);
                    //     while (step.Continue)
                    //     {
                    //         if (verifier.RunFailed(executionId, key, step.Candidate!))
                    //             result = new ReductionResult(true, step.Candidate!);
                    //         else
                    //             result = new ReductionResult(false, result.Value!);
                    //         step = reducer(result);
                    //     }
                    //     shrinkCount++;
                    //     //executionMemory.StoreInputRedux(key, step.Result!);
                    // }
                }
                keptAtLeastOne = keptAtLeastOne || State.Memory.HasShrinker(info.ExecutionId, info.Key, info.Type);
                var (unused, types) = executionMemory.HasUnusedShrinkers(key);
                if (unused)
                {
                    var noun = types.Count == 1 ? "shrinker" : "shrinkers";
                    executionMemory.StoreWarning($"UnusedShrinkers-{executionId}", () => new UnusedShrinkerWarning(types, key));
                }
            }
            if (hasAtLeastOne && !keptAtLeastOne)
                executionMemory.StoreWarning($"AllInputsShrunk-{executionId}", () => new AllInputsIrrelevantWarning());
        }
        return shrinkCount;
    }
}
