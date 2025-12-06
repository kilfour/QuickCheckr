using QuickCheckr.UnderTheHood.Docketing;

namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Primitives;

public class EnumShrinkStrategy
{
    public IEnumerable<InputShrinkingClue> Shrink<T>(CheckrState state, CheckrOf<Case> check, int executionId, string key, T value, string fullKey)
    {
        var verifier = new Verifier(state, check);
        var actualType = typeof(T) == typeof(object) && value != null
            ? value.GetType()
            : typeof(T);

        var enumVals = GetEnumValues(actualType);
        // var originalFails = state.VerifyIf.RunFailed(key, value!);
        // if (!originalFails)
        //     return;

        foreach (var candidate in enumVals.Where(x => !Equals(x, value)))
        {
            if (verifier.RunPassed(executionId, key, candidate))
            {
                var clue = InputShrinkingClue.Keep<EnumShrinkStrategy>(executionId, fullKey, value, state.Editor.IntroduceThis(value));
                state.Docket.Record(new InputShrinkingClueFound(clue));
                yield return clue;
                yield break;
            }
        }
        var ignoreClue = InputShrinkingClue.Ignore<EnumShrinkStrategy>(executionId, fullKey, value);
        state.Docket.Record(new InputShrinkingClueFound(ignoreClue));
        yield return ignoreClue;
    }

    private static IEnumerable<object> GetEnumValues(Type type)
    {
        if (!type.IsEnum) return Array.Empty<object>();
        return Enum.GetValues(type).Cast<object>();
    }
}
