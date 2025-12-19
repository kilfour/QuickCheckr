using QuickCheckr.UnderTheHood;
using QuickFuzzr;

namespace QuickCheckr;

public static partial class Checkr
{
    /// <summary>
    /// Chooses one of the provided Checkr scripts whose condition evaluates to true, and executes it as the next step.  
    /// Use when modelling behaviour where only certain actions are valid in the current state.
    /// </summary>
    public static CheckrOf<Case> OneOfWhen(params (Func<bool> condition, CheckrOf<Case> script)[] scripts)
    {
        var key = $"OneOfWhen|Default|QuickCheckr|Key";
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
                return CheckrResult.CaseOnly(state);
            };
    }
}