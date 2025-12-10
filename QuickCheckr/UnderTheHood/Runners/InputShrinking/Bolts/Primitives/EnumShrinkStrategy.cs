using QuickCheckr.UnderTheHood.Docketing;

namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Primitives;

public class EnumShrinkStrategy
{
    public IEnumerable<InputShrinkingClue> Shrink(CheckrState state, CheckrOf<Case> check, ShrinkingInfo info)
    {
        var verifier = new Verifier(state, check);
        var actualType = info.value != null
            ? info.value.GetType()
            : typeof(object);

        var enumVals = GetEnumValues(actualType);
        // var originalFails = state.VerifyIf.RunFailed(key, value!);
        // if (!originalFails)
        //     return;

        foreach (var candidate in enumVals.Where(x => !Equals(x, info.value)))
        {
            if (verifier.RunPassed(info.executionId, info.key, candidate))
            {
                var clue = InputShrinkingClue.Keep<EnumShrinkStrategy>(info.executionId, info.fullKey, info.value, state.Editor.IntroduceThis(info.value));
                state.Docket.Record(new InputShrinkingClueFound(clue));
                yield return clue;
                yield break;
            }
        }
        var ignoreClue = InputShrinkingClue.Ignore<EnumShrinkStrategy>(info.executionId, info.fullKey, info.value);
        state.Docket.Record(new InputShrinkingClueFound(ignoreClue));
        yield return ignoreClue;
    }

    private static IEnumerable<object> GetEnumValues(Type type)
    {
        if (!type.IsEnum) return Array.Empty<object>();
        return Enum.GetValues(type).Cast<object>();
    }
}
