using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Custom;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Defaults;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Records;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.InputReduction;

namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts;

public class ClassifierSelector
{
    public InputClassification Classify(ShrinkingInfo info, ShrinkingContext context)
    {
        InputClassification classification;
        var shrinkOptions = context.State.Memory.GetClassificationOptions(info.ExecutionId, info.Key, info.Type);
        if (shrinkOptions.Any())
            classification = ClassifyCustom(info, shrinkOptions, context);
        else
            classification = ClassifyDefault(info, context);
        if (classification is InputRelevant)
        {
            var (redux, reduxValue) = GetRedux(info, context);
            classification = classification with { Redux = redux, ReduxValue = reduxValue };
        }
        if (classification is ObjectRelevant objectRelevant)
        {
            if (context.State.ClassificationFormatter.ObjectClassificationFormats.TryGetValue(info.ActualType, out var format))
            {
                var redux = CustomizedPrinting.GetReduxReport(format, objectRelevant.Members);
                classification = classification with { Redux = Maybe.Just(redux) };
            }
        }
        return classification;
    }

    private static (Maybe, Maybe<object>) GetRedux(ShrinkingInfo info, ShrinkingContext context)
    {
        var reducerFactory = context.State.Memory.GetReducerByType(info.Key, info.Type);
        reducerFactory ??= context.State.Memory.GetReducerByPath(info.Key, info.Path, info.Type);
        if (context.UseDefaultReducers && reducerFactory == null)
        {
            reducerFactory = DefaultReducersRegistry.TryGetReducerFactory(info.Type);
        }
        if (reducerFactory == null) return (Maybe.Nothing(), Maybe<object>.Nothing());
        var reducer = reducerFactory();
        var step = ReductionRunner.ForReducer(reducer, info.Value, a => context.Verifier.RunFailed(info.ExecutionId, info.Key, a));
        if (Equals(info.Value, step.Result))
            return (Maybe.Nothing(), Maybe<object>.Nothing());
        context.State.Memory.Commit(info.ExecutionId, info.Key, step.Result!);
        context.State.Memory.Testify(info.ExecutionId, info.Key, step.Result!);
        return (
            Maybe.Just(context.PrintRedux(step.Result!)),
            Maybe<object>.Just(step.Result!));
    }

    private InputClassification ClassifyCustom(ShrinkingInfo info, IEnumerable<IClassificationOption> options, ShrinkingContext context)
        => new CustomClassifierDispatcher(new DefaultClassifierDispatcher()).Dispatch(options, new CustomShrinkingContext(info, context));

    // TODO: private readonly DefaultClassifier defaultClassifier = new();

    public InputClassification ClassifyDefault(ShrinkingInfo info, ShrinkingContext context)
        => new DefaultClassifierDispatcher().Dispatch(info, context);
}
