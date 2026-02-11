using System.Collections;
using QuickCheckr.UnderTheHood.Docketing;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Collections;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Custom;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Objects;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Primitives;
using QuickCheckr.UnderTheHood.Warnings;
using QuickFuzzr;

namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts;

public class ShrinkStrategyPicker
{
    public InputShrinkingClassification Classify(CheckrState state, CheckrOf<Case> check, ShrinkingInfo info)
    {
        var shrinkOptions = state.Memory.GetShrinkOptions(info.ExecutionId, info.Key, info.Type);
        if (shrinkOptions.Any())
            return ClassifyCustom(state, check, info, shrinkOptions);
        return ClassifyDefault(state, check, info);
    }

    private InputShrinkingClassification ClassifyCustom(CheckrState state, CheckrOf<Case> check, ShrinkingInfo info, IEnumerable<IShrinkOption> options)
    {
        foreach (var option in options)
        {
            if (option is IShrinkOptionFuzzr optionFuzzr)
            {
                var shrinker = optionFuzzr.GetFuzzr().Unique(Guid.NewGuid())
                    .Many(optionFuzzr.Times)(state.FuzzrState).Value.Cast<object>();
                var classification = new CustomShrinkStrategy(a => shrinker).Classify(state, check, info);
                if (classification is not InputAssumedIrrelevant)
                    return classification;
            }

            if (option is IShrinkOptionFunction optionCustom)
            {
                var classification = new CustomShrinkStrategy(optionCustom.GetFunction()).Classify(state, check, info);
                if (classification is not InputAssumedIrrelevant)
                    return classification;
            }

            if (option is IShrinkOptionDefault)
            {
                var classification = ClassifyDefault(state, check, info);
                if (classification is not InputAssumedIrrelevant)
                    return classification;
            }

            if (option is IShrinkOptionKeep)
            {
                var keep = InputShrinkingClue.Keep<CustomShrinkStrategy>(
                info.ExecutionId, info.Path, info.Value, state.InputPrinter.IntroduceThis(info.Value));
                state.Docket.Record(new InputShrinkingClueFound(keep));
                return new InputRelevant(state.InputPrinter.IntroduceThis(info.Value));
            }

            if (option is IShrinkOptionNone) return new Unclassified();
        }
        var clue = InputShrinkingClue.Ignore<CustomShrinkStrategy>(info.ExecutionId, info.Path, info.Value);
        state.Docket.Record(new InputShrinkingClueFound(clue));
        return new InputAssumedIrrelevant();
    }

    public InputShrinkingClassification ClassifyDefault(CheckrState state, CheckrOf<Case> check, ShrinkingInfo info)
    {
        var normalizedType = Nullable.GetUnderlyingType(info.Type) ?? info.Type;
        var primitiveKey = PrimitiveShrinkStrategy.PrimitiveValues.Keys.FirstOrDefault(k => k.IsAssignableFrom(normalizedType));
        if (primitiveKey != null)
            return new PrimitiveShrinkStrategy().Classify(state, check, info);
        if (info.Type.IsEnum)
            return new EnumShrinkStrategy().Classify(state, check, info);
        if (typeof(IEnumerable).IsAssignableFrom(info.Type))
            return new EnumerableShrinkStrategy().Classify(state, check, info);

        if (info.Type.IsClass)
            return new ObjectShrinkStrategy().Classify(state, check, info);

        var clue = InputShrinkingClue.Keep<NoShrinkerFound>(info.ExecutionId, info.Path, info.Value, state.InputPrinter.IntroduceThis(info.Value));
        state.Docket.Record(new InputShrinkingClueFound(clue));
        state.Memory.StoreWarning(info.Path, () => new CantShrinkWarning(info.Type, info.Path));

        return new InputRelevant(state.InputPrinter.IntroduceThis(info.Value));
    }

    public class NoShrinkerFound { }
}
