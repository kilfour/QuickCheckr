using QuickCheckr.UnderTheHood.Dispatching;
using QuickCheckr.UnderTheHood.Docketing;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Records;

namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Defaults;


public class EnumClassifier : Classifier
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
        foreach (var candidate in enumVals.Where(x => !Equals(x, info.Value)))
        {
            if (verifier.RunPassed(info.ExecutionId, info.Key, candidate))
            {
                // Mark input as relevant, because we found a candidate that makes the run pass.
                var clue = InputShrinkingClue.Keep<EnumClassifier>(info.ExecutionId, info.Path, info.Value, context.PrintInput(info.Value));
                context.State.Docket.Record(new InputShrinkingClueFound(clue));
                return new InputRelevant(context.PrintInput(info.Value));
            }
        }
        var ignoreClue = InputShrinkingClue.Ignore<EnumClassifier>(info.ExecutionId, info.Path, info.Value);
        context.State.Docket.Record(new InputShrinkingClueFound(ignoreClue));
        return new InputAssumedIrrelevant();
    }

    private static IEnumerable<object> GetEnumValues(Type type)
    {
        if (!type.IsEnum) return Array.Empty<object>();
        return Enum.GetValues(type).Cast<object>();
    }
}