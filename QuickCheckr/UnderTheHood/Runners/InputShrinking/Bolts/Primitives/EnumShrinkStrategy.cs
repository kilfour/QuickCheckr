using QuickCheckr.UnderTheHood.Docketing;

namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Primitives;

public class EnumShrinkStrategy : IShrinkingStrategy
{
    public IEnumerable<InputShrinkingClue> Shrink(CheckrState state, CheckrOf<Case> check, ShrinkingInfo info)
    {
        var verifier = new Verifier(state, check);
        var actualType = info.Value != null
            ? info.Value.GetType()
            : typeof(object);

        var enumVals = GetEnumValues(actualType);

        foreach (var candidate in enumVals.Where(x => !Equals(x, info.Value)))
        {
            if (verifier.RunPassed(info.ExecutionId, info.Key, candidate))
            {
                var clue = InputShrinkingClue.Keep<EnumShrinkStrategy>(info.ExecutionId, info.FullKey, info.Value, state.Editor.IntroduceThis(info.Value));
                state.Docket.Record(new InputShrinkingClueFound(clue));
                yield return clue;
                yield break;
            }
        }
        var ignoreClue = InputShrinkingClue.Ignore<EnumShrinkStrategy>(info.ExecutionId, info.FullKey, info.Value);
        state.Docket.Record(new InputShrinkingClueFound(ignoreClue));
        yield return ignoreClue;
    }

    private static IEnumerable<object> GetEnumValues(Type type)
    {
        if (!type.IsEnum) return Array.Empty<object>();
        return Enum.GetValues(type).Cast<object>();
    }
}
