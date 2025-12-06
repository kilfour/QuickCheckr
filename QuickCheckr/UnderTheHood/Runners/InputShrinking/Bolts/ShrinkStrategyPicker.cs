using System.Collections;
using QuickCheckr.UnderTheHood.Docketing;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Collections;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Custom;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Objects;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Primitives;

namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts;

public class ShrinkStrategyPicker
{
    public static IEnumerable<InputShrinkingClue> Input<T>(CheckrState state, CheckrOf<Case> check, int executionId, string key, T value, string fullKey)
    {
        var customShrinker = state.Memory.GetShrinker<T>(executionId, key);
        if (customShrinker != null)
            return new CustomShrinkStrategy<T>(customShrinker).Shrink(state, check, executionId, key, value, fullKey);

        var actualType = typeof(T) == typeof(object) && value != null
            ? value.GetType()
            : typeof(T);
        var normalizedType = Nullable.GetUnderlyingType(actualType) ?? actualType;

        var primitiveKey = PrimitiveShrinkStrategy.PrimitiveValues.Keys.FirstOrDefault(k => k.IsAssignableFrom(normalizedType));
        if (primitiveKey != null)
            return new PrimitiveShrinkStrategy().Shrink(state, check, executionId, key, value, fullKey).ToList();

        if (actualType.IsEnum)
            return new EnumShrinkStrategy().Shrink(state, check, executionId, key, value, fullKey);

        if (typeof(IEnumerable).IsAssignableFrom(actualType))
            return new EnumerableShrinkStrategy().Shrink(state, check, executionId, key, value, fullKey);

        if (actualType.IsClass)
            return new ObjectShrinkStrategy().Shrink(state, check, executionId, key, value, fullKey);

        var clue = InputShrinkingClue.Keep<NoShrinkerFound>(executionId, fullKey, value, state.Editor.IntroduceThis(value));
        state.Docket.Record(new InputShrinkingClueFound(clue));
        return [clue];
    }
    public class NoShrinkerFound { }
}
