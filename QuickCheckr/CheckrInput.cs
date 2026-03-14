using QuickCheckr.UnderTheHood;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.InputReduction;
using QuickCheckr.UnderTheHood.WhenThingsGoWrong;
using QuickFuzzr;

namespace QuickCheckr;

public static partial class Checkr
{
    public static CheckrOf<T> Input<T>(string key, FuzzrOf<T> fuzzr) =>
        state => HandleInput(state, key, fuzzr);

    public static CheckrOf<T> Input<T>(string key, FuzzrOf<T> fuzzr, params Shrinker[] shrinkers)
    {
        return state =>
        {
            state.Memory.StoreClassification(key, () => [.. shrinkers.SelectMany(s => s.Classifiers())]);
            foreach (var reducer in shrinkers.Where(a => a.Reducers is not null).SelectMany(a => a.Reducers!))
            {
                if (reducer is ITypeReducerOption typeReducer)
                    state.Memory.StoreReducerByType(key, typeReducer.TargetType, typeReducer.ReducerFactory);

                // Existing call : state.Memory.StoreReducerByPath(key, pathReducers);
                // see NestedReducerInObjectOnPath
                // if (reducer is PathReducerFactory pathReducer)
                //       Code below => new shape
                //     state.Memory.StoreReducerByPath(key, pathReducer.TargetType, pathReducer.Path, pathReducer.ReducerFactory);
            }

            return HandleInput(state, key, fuzzr);
        };
    }

    private static CheckrResult<T> HandleInput<T>(CheckrState state, string key, FuzzrOf<T> fuzzr)
    {
        try { return CheckrResult.Some(state, GetValue(key, () => fuzzr, state)!); }
        catch (DuplicateKeyWriteException) { throw; }
        catch (Exception exception) { throw new InputFuzzrThrewException(key, exception); }
    }

    private static T? GetValue<T>(string key, Func<FuzzrOf<T>> factory, CheckrState state)
        => state.Memory.StoreInput(key, () => factory()(state.FuzzrState).Value);
}