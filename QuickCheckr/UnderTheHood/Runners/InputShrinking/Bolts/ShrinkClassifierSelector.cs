using QuickCheckr.UnderTheHood.Docketing;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Custom;
using QuickFuzzr;

namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts;

public class ShrinkClassifierSelector
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
                var classification = new CustomShrinkClassifier(a => shrinker).Classify(state, check, info);
                if (classification is not InputAssumedIrrelevant)
                    return classification;
            }

            if (option is IShrinkOptionFunction optionCustom)
            {
                var classification = new CustomShrinkClassifier(optionCustom.GetFunction()).Classify(state, check, info);
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
                var keep = InputShrinkingClue.Keep<CustomShrinkClassifier>(
                info.ExecutionId, info.Path, info.Value, state.InputPrinter.IntroduceThis(info.Value));
                state.Docket.Record(new InputShrinkingClueFound(keep));
                return new InputRelevant(state.InputPrinter.IntroduceThis(info.Value));
            }

            if (option is IShrinkOptionNone) return new Unclassified();
        }
        var clue = InputShrinkingClue.Ignore<CustomShrinkClassifier>(info.ExecutionId, info.Path, info.Value);
        state.Docket.Record(new InputShrinkingClueFound(clue));
        return new InputAssumedIrrelevant();
    }

    public InputShrinkingClassification ClassifyDefault(CheckrState state, CheckrOf<Case> check, ShrinkingInfo info)
        => new DefaultClassifier()
            .Dispatch(info, new ShrinkingContext(state, check));
}
