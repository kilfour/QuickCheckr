using QuickCheckr.UnderTheHood;

namespace QuickCheckr;

public static partial class Checkr
{
    public static CheckrOf<Case> Sequence(params CheckrOf<Case>[] scripts)
        => Sequence(GetKeyFor.CheckrOneOf, scripts);

    public static CheckrOf<Case> Sequence(string key, params CheckrOf<Case>[] scripts)
    {
        var current = 0;
        var bound = scripts.Length - 1;
        return state =>
        {
            var index = state.Memory.StoreVariable(key, () => current);
            current++;
            if (current > bound)
                current = 0;
            return scripts[index](state);
        };
    }
}