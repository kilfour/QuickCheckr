using System.Collections;
using QuickCheckr.UnderTheHood.Docketing;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Collections;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Objects;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Primitives;
using QuickCheckr.UnderTheHood.Warnings;

namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts;

public class ShrinkStrategyPickerDefault : IShrinkingStrategy
{
    public IEnumerable<InputShrinkingClue> Shrink(CheckrState state, CheckrOf<Case> check, ShrinkingInfo info)
    {
        var normalizedType = Nullable.GetUnderlyingType(info.Type) ?? info.Type;
        var primitiveKey = PrimitiveShrinkStrategy.PrimitiveValues.Keys.FirstOrDefault(k => k.IsAssignableFrom(normalizedType));

        if (primitiveKey != null)
            return [.. new PrimitiveShrinkStrategy().Shrink(state, check, info)];

        if (info.Type.IsEnum)
            return new EnumShrinkStrategy().Shrink(state, check, info);

        if (typeof(IEnumerable).IsAssignableFrom(info.Type))
            return new EnumerableShrinkStrategy().Shrink(state, check, info);

        if (info.Type.IsClass)
            return new ObjectShrinkStrategy().Shrink(state, check, info);

        var clue = InputShrinkingClue.Keep<NoShrinkerFound>(info.ExecutionId, info.Path, info.Value, state.InputPrinter.IntroduceThis(info.Value));
        state.Docket.Record(new InputShrinkingClueFound(clue));
        state.Memory.StoreWarning(info.Path, () => new CantShrinkWarning(info.Type, info.Path));
        return [clue];
    }

    public class NoShrinkerFound { }
}
