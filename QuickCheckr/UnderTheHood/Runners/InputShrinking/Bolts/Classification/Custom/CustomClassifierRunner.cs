using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Records;

namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Custom;

internal sealed class CustomClassifierRunner(Func<object, IEnumerable<object>> shrinker)
{
    public async ValueTask<InputClassification> ClassifyAsync(ShrinkingInfo info, ShrinkingContext context)
    {
        var verifier = new Verifier(context.State, context.Check);
        var values = shrinker(info.Value!);
        var applicableCandidate = false;
        var rejectedCandidate = false;
        foreach (var candidate in values.Where(x => !Equals(x, info.Value)))
        {
            switch (await verifier.RunAsync(info.ExecutionId, info.Key, candidate!).ConfigureAwait(false))
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
