using QuickCheckr;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Defaults;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Records;
using QuickCheckr.UnderTheHood.Warnings;

namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.InputReduction;

public class ReducerSelector
{
    private static string GetCollectionReport(IEnumerable<InputClassification> elements)
    {
        var values =
            elements.Where(a => a is not ElementAssumedIrrelevant)
                .Select(a => GetReportValue(a))
                .ToList();
        if (values.Count == 0) return "[ ]";
        return $"[ {string.Join(", ", values)} ]";
    }

    private static string GetObjectReport(Dictionary<string, InputClassification> members) =>
         $"{{ {string.Join(", ",
            members.Where(a => a.Value is InputRelevant)
                .Select(kvp => $"{kvp.Key}: {GetValue(kvp)}"))} }}";

    private static string GetTupleReport(Dictionary<string, InputClassification> members) =>
        $"( {string.Join(", ",
            members
                .Select(kvp => $"{GetValue(kvp)}"))} )";

    private static string GetValue(KeyValuePair<string, InputClassification> kvp) =>
        kvp.Value switch
        {
            InputRelevant => GetReport(kvp),
            _ => "_"
        };

    private static string GetReport(KeyValuePair<string, InputClassification> kvp)
    {
        if (kvp.Value is InputRelevant inputRelevant)
        {
            if (inputRelevant.Redux.HasValue)
                return inputRelevant.Redux.Value!.Report;
            return inputRelevant.Report;
        }
        return "_";
    }

    private static string GetReportValue(InputClassification classification)
    {
        if (classification is InputRelevant inputRelevant)
        {
            if (inputRelevant.Redux.HasValue)
                return inputRelevant.Redux.Value!.Report;
            return inputRelevant.Report;
        }
        return "_";
    }

    public static InputClassification Reduce(ShrinkingInfo info, ShrinkingContext context, InputClassification classification)
    {

        if (classification is CollectionRelevant collectionRelevant)
        {
            var redux = GetRedux(info, context);
            if (!redux.HasValue)
                redux = Maybe<Redux>.Just(new Redux(GetCollectionReport(collectionRelevant.Elements), info.Value!));
            if (redux.HasValue && collectionRelevant.Report != redux.Value!.Report)
                classification = collectionRelevant with { Redux = redux };
            return classification;
        }

        if (classification is TupleRelevant tupleRelevant)
        {
            var redux = GetRedux(info, context);
            if (!redux.HasValue)
                redux = Maybe<Redux>.Just(new Redux(GetTupleReport(tupleRelevant.Members), info.Value!));
            if (redux.HasValue && tupleRelevant.Report != redux.Value!.Report)
                classification = tupleRelevant with { Redux = redux };
            return classification;
        }

        if (classification is ObjectRelevant objectRelevant)
        {
            var redux = GetRedux(info, context);
            if (!redux.HasValue)
            {
                string report;
                if (context.State.ClassificationFormatter.ObjectClassificationFormats.TryGetValue(info.ActualType, out var format))
                    report = CustomizedPrinting.GetReduxReport(format, objectRelevant.Members);
                else
                    report = GetObjectReport(objectRelevant.Members);
                redux = Maybe<Redux>.Just(new Redux(report, info.Value!));
            }
            if (redux.HasValue && objectRelevant.Report != redux.Value!.Report)
                classification = objectRelevant with { Redux = redux };
            return classification;
        }
        if (classification is InputRelevant inputRelevant)
        {
            classification = inputRelevant with { Redux = GetRedux(info, context) };
        }

        return classification;
    }

    private static Maybe<Redux> GetRedux(ShrinkingInfo info, ShrinkingContext context)
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
        return Maybe<Redux>.Nothing();
    }

    private static Maybe<Redux> Reduce(
        ShrinkingInfo info,
        ShrinkingContext context,
        IEnumerable<Func<Func<ReductionResult, Reduction>>> reducerFactories)
    {
        var reducer = reducerFactories.First()();
        var step = ReductionRunner.ForReducer(
            reducer,
            info.Key,
            info.Path,
            info.Value,
            a => context.Verifier.TestimonyFailed(info.ExecutionId, info.Key, a),
            context);
        var witnessed = step is Reduction.Stop;
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
            step = ReductionRunner.ForReducer(
                reducer,
                info.Key,
                info.Path,
                value,
                a => context.Verifier.TestimonyFailed(info.ExecutionId, info.Key, a),
                context);
            witnessed = witnessed || step is Reduction.Stop;
        }
        if (witnessed && step is Reduction.Unbounded finalUnbounded)
            return ReduxFromValue(info, context, finalUnbounded.Result);
        return ReduxFromStep(info, context, step);
    }

    private static Maybe<Redux> Reduce(ShrinkingInfo info, ShrinkingContext context, Func<Func<ReductionResult, Reduction>> reducerFactory)
    {
        var reducer = reducerFactory();
        var step = ReductionRunner.ForReducer(
            reducer,
            info.Key,
            info.Path,
            info.Value,
            a => context.Verifier.TestimonyFailed(info.ExecutionId, info.Key, a),
            context);
        return ReduxFromStep(info, context, step);
    }

    private static Maybe<Redux> ReduxFromStep(ShrinkingInfo info, ShrinkingContext context, Reduction step)
    {
        if (step is Reduction.Unbounded)
        {
            context.Warn(info.Path, () => new NoWitnessForReducerWarning(info.Path));
        }
        return step switch
        {
            Reduction.Stop stop => ReduxFromValue(info, context, stop.Result),
            Reduction.Unbounded unbounded => ReduxFromValue(info, context, unbounded.Result),
            Reduction.Next _ => throw new InvalidOperationException("Reduction.Next should not be the end result of a reduction."),
            _ => throw new InvalidOperationException(),
        };
    }

    private static Maybe<Redux> ReduxFromValue(ShrinkingInfo info, ShrinkingContext context, object? value)
    {
        if (Equals(info.Value, value))
            return Maybe<Redux>.Nothing();
        context.State.Memory.Commit(info.ExecutionId, info.Key, value!);
        context.State.Memory.Testify(info.ExecutionId, info.Key, value!);
        return Maybe<Redux>.Just(new Redux(
            context.PrintRedux(value!),
            value!
        ));
    }
}
