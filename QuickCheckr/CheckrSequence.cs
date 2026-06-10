using QuickCheckr.UnderTheHood;

namespace QuickCheckr;

public static partial class Checkr
{
    public static CheckrOf<Case> Sequence(params CheckrOf<Case>[] scripts)
        => Sequence(GetKeyFor.CheckrSequence, scripts);

    public static CheckrOf<Case> Sequence(string key, params CheckrOf<Case>[] scripts)
    {
        var bound = scripts.Length - 1;
        return state =>
        {
            var current = state.Memory.GetLongTermVariable(key, 0);
            var index = state.Memory.StoreVariable(key, () => current);
            current++;
            if (current > bound)
                current = 0;
            state.Memory.StoreLongTermVariable(key, current);
            return scripts[index](state);
        };
    }
}