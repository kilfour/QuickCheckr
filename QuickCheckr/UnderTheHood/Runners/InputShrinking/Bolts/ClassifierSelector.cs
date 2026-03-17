using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Custom;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Defaults;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Records;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.InputReduction;

namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts;

public class ClassifierSelector
{
    public InputClassification Classify(ShrinkingInfo info, ShrinkingContext context)
    {
        InputClassification? classification = null;
        var shrinkers = context.State.Memory.GetShrinkers(info.ExecutionId, info.Key, info.Type);
        if (shrinkers is not null)
        {
            var shrinker = shrinkers.Where(a => a.Classifiers.Length != 0).FirstOrDefault(x => x.Condition(info));
            if (shrinker is not null)
                classification = ClassifyCustom(info, shrinker.Classifiers, context);
        }
        classification ??= ClassifyDefault(info, context);
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
        var shrinkers = context.State.Memory.GetShrinkers(info.ExecutionId, info.Key, info.Type);
        if (shrinkers is not null)
        {
            var shrinker = shrinkers.Where(a => a.Reducers.Length != 0).FirstOrDefault(x => x.Condition(info));
            if (shrinker is not null)
            {
                var reducerFactory = shrinker.Reducers.First().ReducerFactory;
                return Reduce(info, context, reducerFactory);
            }
        }
        if (context.UseDefaultReducers)
        {
            var reducerFactory = DefaultReducersRegistry.TryGetReducerFactory(info.Type);
            if (reducerFactory is not null)
                return Reduce(info, context, reducerFactory);
        }
        return (Maybe.Nothing(), Maybe<object>.Nothing());
    }

    private static (Maybe, Maybe<object>) Reduce(ShrinkingInfo info, ShrinkingContext context, Func<Func<ReductionResult, Reduction>> reducerFactory)
    {
        var reducer = reducerFactory();
        var step = ReductionRunner.ForReducer(reducer, info.Value, a => context.Verifier.TestimonyFailed(info.ExecutionId, info.Key, a));
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
