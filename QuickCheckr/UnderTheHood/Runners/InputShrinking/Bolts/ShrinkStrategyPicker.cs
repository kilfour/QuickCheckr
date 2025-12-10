using System.Collections;
using QuickCheckr.UnderTheHood.Docketing;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Collections;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Custom;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Objects;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Primitives;

namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts;

public class ShrinkStrategyPicker
{
    public static IEnumerable<InputShrinkingClue> Input(CheckrState state, CheckrOf<Case> check, ShrinkingInfo info)
    {
        var customShrinker = state.Memory.GetShrinker(info.executionId, info.key);
        if (customShrinker != null)
            return new CustomShrinkStrategy(customShrinker).Shrink(state, check, info);

        var actualType = info.value != null
            ? info.value.GetType()
            : typeof(object);
        var normalizedType = Nullable.GetUnderlyingType(actualType) ?? actualType;

        var primitiveKey = PrimitiveShrinkStrategy.PrimitiveValues.Keys.FirstOrDefault(k => k.IsAssignableFrom(normalizedType));
        if (primitiveKey != null)
            return [.. new PrimitiveShrinkStrategy().Shrink(state, check, info)];

        if (actualType.IsEnum)
            return new EnumShrinkStrategy().Shrink(state, check, info);

        if (typeof(IEnumerable).IsAssignableFrom(actualType))
            return new EnumerableShrinkStrategy().Shrink(state, check, info);

        if (actualType.IsClass)
            return new ObjectShrinkStrategy().Shrink(state, check, info);

        var clue = InputShrinkingClue.Keep<NoShrinkerFound>(info.executionId, info.fullKey, info.value, state.Editor.IntroduceThis(info.value));
        state.Docket.Record(new InputShrinkingClueFound(clue));
        return [clue];
    }
    public class NoShrinkerFound { }
}
