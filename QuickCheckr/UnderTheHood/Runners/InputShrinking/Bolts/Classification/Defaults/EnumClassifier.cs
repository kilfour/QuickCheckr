using QuickCheckr.UnderTheHood.Dispatching;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Records;

namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Defaults;


internal sealed class EnumClassifier : Classifier
{
    public override HandlerResult<InputClassification> TryHandle(ShrinkingInfo info, ShrinkingContext context)
    {
        if (!CanHandle(info)) return Unhandled;
        return HandlerResult.Handled(Handle(info, context));
    }

    private static bool CanHandle(ShrinkingInfo info) => info.Type.IsEnum;

    private static InputClassification Handle(ShrinkingInfo info, ShrinkingContext context)
    {
        var verifier = new Verifier(context.State, context.Check);
        var enumVals = GetEnumValues(info.Type);
        var applicableCandidate = false;
        var rejectedCandidate = false;
        foreach (var candidate in enumVals.Where(x => !Equals(x, info.Value)))
        {
            switch (verifier.Run(info.ExecutionId, info.Key, candidate))
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
