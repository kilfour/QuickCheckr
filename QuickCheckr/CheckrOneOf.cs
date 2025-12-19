using QuickFuzzr;

namespace QuickCheckr;

public static partial class Checkr
{
    /// <summary>
    /// Chooses one of the provided Checkr scripts at random and executes it as the next step.  
    /// Use when modelling behaviour where multiple alternative actions may occur, each with equal probability.
    /// </summary>
    public static CheckrOf<Case> OneOf(params CheckrOf<Case>[] scripts)
        => OneOf("OneOf|Default|QuickCheckr|Key", scripts);


    public static CheckrOf<Case> OneOf(string key, params CheckrOf<Case>[] scripts) =>
        state =>
        {
            var index = state.Memory.StoreVariable(key,
                () => Fuzzr.Int(0, scripts.Length)(state.FuzzrState).Value);
            return scripts[index](state);
        };
}