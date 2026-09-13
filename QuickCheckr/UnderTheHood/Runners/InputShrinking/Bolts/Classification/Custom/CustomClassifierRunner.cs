using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Records;

namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Custom;

public class CustomClassifierRunner(Func<object, IEnumerable<object>> shrinker)
{
    public InputClassification Classify(ShrinkingInfo info, ShrinkingContext context)
    {
        var verifier = new Verifier(context.State, context.Check);
        var values = shrinker(info.Value!);
        var applicableCandidate = false;
        var rejectedCandidate = false;
        foreach (var candidate in values.Where(x => !Equals(x, info.Value)))
        {
            switch (verifier.Run(info.ExecutionId, info.Key, candidate!))
            {
                case ReplayOutcome.Passed:
                    return InputRelevant.Create(context.PrintInput(info.Value));
                case ReplayOutcome.FailurePreserved:
                    applicableCandidate = true;
                    break;
                case ReplayOutcome.CandidateRejected:
                    rejectedCandidate = true;
                    break;
            }
        }
        if (rejectedCandidate && !applicableCandidate)
            return InputRelevant.Create(context.PrintInput(info.Value));
        return new InputAssumedIrrelevant();
    }
}
