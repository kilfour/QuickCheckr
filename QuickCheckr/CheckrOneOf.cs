using QuickCheckr.UnderTheHood;
using QuickFuzzr;

namespace QuickCheckr;

public static partial class Checkr
{
    /// <summary>
    /// Chooses one of the provided Checkr scripts at random and executes it as the next step.  
    /// Use when modelling behaviour where multiple alternative actions may occur, each with equal probability.
    /// </summary>
    public static CheckrOf<T> OneOf<T>(params CheckrOf<T>[] scripts)
        => OneOfInternal(scripts);

    private static CheckrOf<T> OneOfInternal<T>(
        CheckrOf<T>[] scripts)
    {
        var key = $"OneOf|Default|QuickCheckr|Key";
        return state =>
            {
                var index = state.Memory.StoreVariable(key,
                    () => Fuzzr.Int(0, scripts.Length)(state.FuzzrState).Value);
                return scripts[index](state);
            };
    }

    /// <summary>
    /// Chooses one of the provided Checkr scripts whose condition evaluates to true, and executes it as the next step.  
    /// Use when modelling behaviour where only certain actions are valid in the current state.
    /// </summary>
    public static CheckrOf<T> OneOfIf<T>(params (Func<bool> condition, CheckrOf<T> script)[] scripts)
        => OneOfIfInternal(scripts);

    public static CheckrOf<T> OneOfIf<T>(params CheckrOf<(Func<bool> condition, CheckrOf<T> script)>[] scriptsC)
    {
        var key = $"OneOf|Default|QuickCheckr|Key";
        return state =>
            {
                var scripts = scriptsC.Select(a => a(state).Value).ToArray();
                var validScriptIndexes =
                    scripts.Select((a, ix) => (ix, a.condition))
                        .Where(a => a.condition())
                        .Select(a => a.ix);
                var index = state.Memory.StoreVariable(key,
                    () => Fuzzr.OneOf(validScriptIndexes)(state.FuzzrState).Value);
                var script = scripts[index];
                if (script.condition())
                    return scripts[index].script(state);
                return CheckrResult.Default<T>(state);
            };
    }

    private static CheckrOf<T> OneOfIfInternal<T>(
         (Func<bool> condition, CheckrOf<T> script)[] scripts)
    {
        var key = $"OneOf|Default|QuickCheckr|Key";
        return state =>
            {
                var validScriptIndexes =
                    scripts.Select((a, ix) => (ix, a.condition))
                        .Where(a => a.condition())
                        .Select(a => a.ix);
                var index = state.Memory.StoreVariable(key,
                    () => Fuzzr.OneOf(validScriptIndexes)(state.FuzzrState).Value);
                var script = scripts[index];
                // if (state.EvidenceRecorder.IsReplay || script.condition())
                if (script.condition())
                    return scripts[index].script(state);
                return CheckrResult.Default<T>(state);
            };
    }
}