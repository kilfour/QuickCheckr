using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Custom;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Defaults;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Records;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.InputReduction;

namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts;

public class ShrinkClassifierSelector
{
    public InputShrinkingClassification Classify(ShrinkingInfo info, ShrinkingContext context)
    {
        InputShrinkingClassification classification;
        var shrinkOptions = context.State.Memory.GetShrinkOptions(info.ExecutionId, info.Key, info.Type);
        if (shrinkOptions.Any())
            classification = ClassifyCustom(info, shrinkOptions, context);
        else
            classification = ClassifyDefault(info, context);
        if (classification is InputRelevant)
        {
            var (redux, reduxValue) = GetRedux(info, context);
            classification = classification with { Redux = redux, ReduxValue = reduxValue };
        }
        return classification;
    }

    private (Maybe, Maybe<object>) GetRedux(ShrinkingInfo info, ShrinkingContext context)
    {
        var reducerFactory = context.State.Memory.GetReducer(info.Path);
        if (reducerFactory == null) return (Maybe.Nothing(), Maybe<object>.Nothing());
        var reducer = reducerFactory();
        var result = new ReductionResult(true, info.Value);
        Reduction step = Reduction.Next(info.Value!);
        while (step.Continue)
        {
            if (context.Verifier.RunFailed(info.ExecutionId, info.Key, step.Candidate!))
                result = new ReductionResult(true, step.Candidate!);
            else
                result = new ReductionResult(false, result.Value!);
            step = reducer(result);
        }
        return (
            Maybe.Just(context.State.InputPrinter.IntroduceThis(step.Result!)),
            Maybe<object>.Just(step.Result!));
    }

    private InputShrinkingClassification ClassifyCustom(ShrinkingInfo info, IEnumerable<IShrinkOption> options, ShrinkingContext context)
        => new CustomClassifierDispatcher(new DefaultClassifierDispatcher()).Dispatch(options, new CustomShrinkingContext(info, context));

    // TODO: private readonly DefaultClassifier defaultClassifier = new();

    public InputShrinkingClassification ClassifyDefault(ShrinkingInfo info, ShrinkingContext context)
        => new DefaultClassifierDispatcher().Dispatch(info, context);
}
