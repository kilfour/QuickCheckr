using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Records;

namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Custom;

public class CustomClassifierRunner(Func<object, IEnumerable<object>> shrinker)
{
    public InputClassification Classify(ShrinkingInfo info, ShrinkingContext context)
    {
        var verifier = new Verifier(context.State, context.Check);
        var values = shrinker(info.Value!);
        foreach (var candidate in values.Where(x => !Equals(x, info.Value)))
        {
            if (verifier.RunPassed(info.ExecutionId, info.Key, candidate!))
            {
                return InputRelevant.Create(context.PrintInput(info.Value));
            }
        }
        return new InputAssumedIrrelevant();
    }
}