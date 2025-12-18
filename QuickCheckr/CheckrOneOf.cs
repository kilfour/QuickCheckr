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
}