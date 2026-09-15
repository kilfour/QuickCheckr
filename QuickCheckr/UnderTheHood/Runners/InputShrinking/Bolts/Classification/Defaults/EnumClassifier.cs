using QuickCheckr.UnderTheHood.Dispatching;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Records;

namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Defaults;


internal sealed class EnumClassifier : Classifier
{
    public override async ValueTask<HandlerResult<InputClassification>> TryHandleAsync(ShrinkingInfo info, ShrinkingContext context)
    {
        if (!CanHandle(info)) return Unhandled;
        return HandlerResult.Handled(await HandleAsync(info, context).ConfigureAwait(false));
    }

    private static bool CanHandle(ShrinkingInfo info) => info.Type.IsEnum;

    private static async ValueTask<InputClassification> HandleAsync(ShrinkingInfo info, ShrinkingContext context)
    {
        var verifier = new Verifier(context.State, context.Check);
        var enumVals = GetEnumValues(info.Type);
        var applicableCandidate = false;
        var rejectedCandidate = false;
        foreach (var candidate in enumVals.Where(x => !Equals(x, info.Value)))
        {
            switch (await verifier.RunAsync(info.ExecutionId, info.Key, candidate).ConfigureAwait(false))
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

    private static IEnumerable<object> GetEnumValues(Type type)
    {
        if (!type.IsEnum) return Array.Empty<object>();
        return Enum.GetValues(type).Cast<object>();
    }
}
