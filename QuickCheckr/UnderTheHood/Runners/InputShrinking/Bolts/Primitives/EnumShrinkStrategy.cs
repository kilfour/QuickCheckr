using QuickCheckr.UnderTheHood.Docketing;

namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Primitives;

public class EnumShrinkStrategy : IShrinkingStrategy
{
    public IEnumerable<InputShrinkingClue> Shrink(CheckrState state, CheckrOf<Case> check, ShrinkingInfo info)
    {
        var verifier = new Verifier(state, check);
        var enumVals = GetEnumValues(info.Type);
        foreach (var candidate in enumVals.Where(x => !Equals(x, info.Value)))
        {
            if (verifier.RunPassed(info.ExecutionId, info.Key, candidate))
            {
                var clue = InputShrinkingClue.Keep<EnumShrinkStrategy>(info.ExecutionId, info.Path, info.Value, state.InputPrinter.IntroduceThis(info.Value));
                state.Docket.Record(new InputShrinkingClueFound(clue));
                return [clue];
            }
        }
        var ignoreClue = InputShrinkingClue.Ignore<EnumShrinkStrategy>(info.ExecutionId, info.Path, info.Value);
        state.Docket.Record(new InputShrinkingClueFound(ignoreClue));
        return [ignoreClue];
    }

    private static IEnumerable<object> GetEnumValues(Type type)
    {
        if (!type.IsEnum) return Array.Empty<object>();
        return Enum.GetValues(type).Cast<object>();
    }
}
