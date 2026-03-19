using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Custom;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Defaults;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Records;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.InputReduction;
using QuickCheckr.UnderTheHood.Warnings;

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
                return Reduce(info, context, shrinker.Reducers.Select(a => a.ReducerFactory));
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

    private static (Maybe, Maybe<object>) Reduce(
        ShrinkingInfo info,
        ShrinkingContext context,
        IEnumerable<Func<Func<ReductionResult, Reduction>>> reducerFactories)
    {
        var reducer = reducerFactories.First()();
        var step = ReductionRunner.ForReducer(reducer, info.Value, a => context.Verifier.TestimonyFailed(info.ExecutionId, info.Key, a));
        foreach (var reducerFactory in reducerFactories.Skip(1))
        {
            reducer = reducerFactory();
            var value = step switch
            {
                Reduction.Next next => next.Candidate,
                Reduction.Stop stop => stop.Result,
                Reduction.Unbounded unbounded => unbounded.Result,
                _ => info.Value
            };
            step = ReductionRunner.ForReducer(reducer, value, a => context.Verifier.TestimonyFailed(info.ExecutionId, info.Key, a));
        }
        return ReduxFromStep(info, context, step);
    }

    private static (Maybe, Maybe<object>) Reduce(ShrinkingInfo info, ShrinkingContext context, Func<Func<ReductionResult, Reduction>> reducerFactory)
    {
        var reducer = reducerFactory();
        var step = ReductionRunner.ForReducer(reducer, info.Value, a => context.Verifier.TestimonyFailed(info.ExecutionId, info.Key, a));
        return ReduxFromStep(info, context, step);
    }

    private static (Maybe, Maybe<object>) ReduxFromStep(ShrinkingInfo info, ShrinkingContext context, Reduction step)
    {
        if (step is Reduction.Unbounded)
        {
            context.State.Memory.StoreWarning(info.Path, () => new NoWitnessForReducerWarning(info.Path));
        }
        return step switch
        {
            Reduction.Stop stop => ReduxFromValue(info, context, stop.Result),
            Reduction.Unbounded unbounded => ReduxFromValue(info, context, unbounded.Result),
            Reduction.Next _ => throw new InvalidOperationException("Reduction.Next should not be the end result of a reduction."),
            _ => throw new InvalidOperationException(),
        };
    }

    private static (Maybe, Maybe<object>) ReduxFromValue(ShrinkingInfo info, ShrinkingContext context, object? value)
    {
        if (Equals(info.Value, value))
            return (Maybe.Nothing(), Maybe<object>.Nothing());
        context.State.Memory.Commit(info.ExecutionId, info.Key, value!);
        context.State.Memory.Testify(info.ExecutionId, info.Key, value!);
        return (
            Maybe.Just(context.PrintRedux(value!)),
            Maybe<object>.Just(value!));
    }

    private InputClassification ClassifyCustom(ShrinkingInfo info, IEnumerable<IClassificationOption> options, ShrinkingContext context)
        => new CustomClassifierDispatcher(new DefaultClassifierDispatcher()).Dispatch(options, new CustomShrinkingContext(info, context));

    // TODO: private readonly DefaultClassifier defaultClassifier = new();

    public InputClassification ClassifyDefault(ShrinkingInfo info, ShrinkingContext context)
        => new DefaultClassifierDispatcher().Dispatch(info, context);
}
